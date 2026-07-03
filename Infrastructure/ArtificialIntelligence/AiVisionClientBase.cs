using System.Net;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Application.DTOs.AiValidation;
using Application.DTOs.AiProviders;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public abstract class AiVisionClientBase : IAiVisionClient
{
    private const int MaxRateLimitRetryCount = 2;

    private static readonly TimeSpan DefaultRateLimitRetryDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxRateLimitRetryDelay = TimeSpan.FromSeconds(10);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogService _logService;

    protected AiVisionClientBase(
        string providerName,
        HttpClient httpClient,
        PromptTemplateProvider promptTemplateProvider,
        AiStructuredOutputSchemaProvider schemaProvider,
        ILogService logService)
    {
        ProviderName = providerName;
        _httpClient = httpClient;
        _logService = logService;
        PromptTemplateProvider = promptTemplateProvider;
        SchemaProvider = schemaProvider;
    }

    public string ProviderName { get; }

    public string PromptVersion => PromptTemplateProvider.Version;

    public string SchemaVersion => SchemaProvider.Version;

    protected PromptTemplateProvider PromptTemplateProvider { get; }

    protected AiStructuredOutputSchemaProvider SchemaProvider { get; }

    public async Task<AiVisionExtractionDto> ExtractSetupAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        AiProviderRuntimeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Uri? endpoint = null;
        IReadOnlyList<AiVisionImagePayload> imagePayloads = [];
        IReadOnlyList<string?> imageDetails = [];
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (!string.Equals(configuration.ProviderName, ProviderName, StringComparison.OrdinalIgnoreCase))
            {
                throw CreateProviderException(configuration, "provider_mismatch", $"AI provider configuration '{configuration.ProviderName}' does not match adapter '{ProviderName}'.");
            }

            if (!Uri.TryCreate(configuration.Endpoint, UriKind.Absolute, out endpoint))
            {
                throw CreateProviderException(configuration, "invalid_endpoint", "AI provider endpoint is not configured with an absolute URL.");
            }

            imagePayloads = await BuildImagePayloadsAsync(images, cancellationToken);
            var providerRequest = BuildProviderRequest(request, imagePayloads, configuration);
            var requestJson = JsonSerializer.Serialize(providerRequest, JsonOptions);
            imageDetails = AiProviderDiagnosticBuilder.ExtractImageDetails(requestJson);

            var apiKey = Environment.GetEnvironmentVariable(configuration.ApiKeyEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw CreateProviderException(configuration, "missing_api_key", $"Missing API key environment variable '{configuration.ApiKeyEnvironmentVariable}'.");
            }

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(configuration.TimeoutSeconds));

            for (var attempt = 0; ; attempt++)
            {
                TimeSpan? retryDelay = null;

                using (var httpRequest = CreateHttpRequest(endpoint, requestJson, apiKey))
                using (var response = await _httpClient.SendAsync(httpRequest, timeoutSource.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync(timeoutSource.Token);
                        var modelContent = ExtractModelContent(responseJson);
                        var extraction = DeserializeExtraction(modelContent);

                        EnsureCompleteExtraction(configuration, extraction);

                        return extraction;
                    }

                    var responseBody = response.Content is null
                        ? null
                        : await response.Content.ReadAsStringAsync(timeoutSource.Token);
                    var diagnostic = AiProviderDiagnosticBuilder.CreateHttpStatusDiagnostic(
                        ProviderName,
                        configuration,
                        endpoint,
                        response,
                        responseBody,
                        stopwatch.Elapsed,
                        imagePayloads,
                        imageDetails);

                    if (ShouldRetryRateLimit(response.StatusCode, diagnostic, attempt))
                    {
                        retryDelay = ResolveRateLimitRetryDelay(response.Headers.RetryAfter);
                    }
                    else
                    {
                        throw CreateHttpStatusException(configuration, response.StatusCode, diagnostic);
                    }
                }

                if (retryDelay.HasValue)
                {
                    await Task.Delay(retryDelay.Value, timeoutSource.Token);
                }
            }
        }
        catch (AiProviderException ex)
        {
            LogProviderFailure(ex);
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var diagnostic = AiProviderDiagnosticBuilder.CreateExceptionDiagnostic(
                ProviderName,
                configuration,
                endpoint,
                "timeout",
                ex,
                stopwatch.Elapsed,
                imagePayloads,
                imageDetails);
            var normalizedException = CreateProviderException(configuration, "timeout", "AI provider request timed out.", innerException: ex, diagnostic: diagnostic);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
        catch (TimeoutException ex)
        {
            var diagnostic = AiProviderDiagnosticBuilder.CreateExceptionDiagnostic(
                ProviderName,
                configuration,
                endpoint,
                "timeout",
                ex,
                stopwatch.Elapsed,
                imagePayloads,
                imageDetails);
            var normalizedException = CreateProviderException(configuration, "timeout", "AI provider request timed out.", innerException: ex, diagnostic: diagnostic);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
        catch (HttpRequestException ex)
        {
            var diagnostic = AiProviderDiagnosticBuilder.CreateExceptionDiagnostic(
                ProviderName,
                configuration,
                endpoint,
                "http_error",
                ex,
                stopwatch.Elapsed,
                imagePayloads,
                imageDetails,
                ex.StatusCode.HasValue ? (int)ex.StatusCode.Value : null);
            var normalizedException = CreateProviderException(configuration, "http_error", "AI provider request failed before a valid response was received.", ex.StatusCode, ex, diagnostic);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
        catch (JsonException ex)
        {
            var diagnostic = AiProviderDiagnosticBuilder.CreateExceptionDiagnostic(
                ProviderName,
                configuration,
                endpoint,
                "invalid_json",
                ex,
                stopwatch.Elapsed,
                imagePayloads,
                imageDetails);
            var normalizedException = CreateProviderException(configuration, "invalid_json", "AI provider response was not valid strict JSON.", innerException: ex, diagnostic: diagnostic);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
    }

    protected abstract object BuildProviderRequest(
        CreateAiValidationDto request,
        IReadOnlyList<AiVisionImagePayload> images,
        AiProviderRuntimeConfiguration configuration);

    protected abstract string ExtractModelContent(string responseJson);

    protected JsonElement GetSchemaJsonElement()
    {
        using var document = JsonDocument.Parse(SchemaProvider.GetSchema());
        return document.RootElement.Clone();
    }

    protected static string BuildTradeContext(CreateAiValidationDto request)
    {
        var comment = string.IsNullOrWhiteSpace(request.UserComment) ? "null" : request.UserComment;

        return $$"""
            Trade setup input:
            {
              "instrumentId": {{request.InstrumentId}},
              "directionId": {{request.DirectionId}},
              "entryPrice": {{request.EntryPrice}},
              "stopLoss": {{request.StopLoss}},
              "takeProfit": {{request.TakeProfit}},
              "userComment": {{JsonSerializer.Serialize(comment)}}
            }

            Extract the visual fields from the attached images and return strict JSON only.
            """;
    }

    protected static string ExtractChatCompletionContent(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (root.TryGetProperty("choices", out var choices))
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    return ReadContentAsString(content);
                }

                if (choice.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("content", out var deltaContent))
                {
                    return ReadContentAsString(deltaContent);
                }
            }
        }

        throw new JsonException("Provider response did not contain a chat completion message.");
    }

    protected static object BuildChatTextPart(string text)
    {
        return new
        {
            type = "text",
            text
        };
    }

    protected static object BuildChatImagePart(AiVisionImagePayload image)
    {
        return new
        {
            type = "image_url",
            image_url = new
            {
                url = image.DataUri
            }
        };
    }

    private static async Task<IReadOnlyList<AiVisionImagePayload>> BuildImagePayloadsAsync(
        IReadOnlyList<AiValidationImageInputDto> images,
        CancellationToken cancellationToken)
    {
        var payloads = new List<AiVisionImagePayload>(images.Count);

        foreach (var image in images.OrderBy(image => image.SortOrder))
        {
            if (image.Content.CanSeek)
            {
                image.Content.Position = 0;
            }

            using var memoryStream = new MemoryStream();
            await image.Content.CopyToAsync(memoryStream, cancellationToken);

            if (image.Content.CanSeek)
            {
                image.Content.Position = 0;
            }

            var base64 = Convert.ToBase64String(memoryStream.ToArray());

            payloads.Add(new AiVisionImagePayload(
                image.OriginalFileName,
                image.ContentType,
                memoryStream.Length,
                $"data:{image.ContentType};base64,{base64}",
                image.FrameCode,
                image.ImageRole.ToString(),
                image.SortOrder,
                image.Comment));
        }

        return payloads;
    }

    private static HttpRequestMessage CreateHttpRequest(Uri endpoint, string requestJson, string apiKey)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        return httpRequest;
    }

    private static bool ShouldRetryRateLimit(
        HttpStatusCode statusCode,
        AiProviderFailureDiagnostic diagnostic,
        int attempt)
    {
        return (int)statusCode == 429 &&
            attempt < MaxRateLimitRetryCount &&
            !string.Equals(diagnostic.ProviderErrorCode, "insufficient_quota", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(diagnostic.ProviderErrorType, "insufficient_quota", StringComparison.OrdinalIgnoreCase);
    }

    private static TimeSpan ResolveRateLimitRetryDelay(RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter?.Delta is { } delta && delta >= TimeSpan.Zero)
        {
            return CapRateLimitRetryDelay(delta);
        }

        if (retryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return CapRateLimitRetryDelay(delay);
            }
        }

        return DefaultRateLimitRetryDelay;
    }

    private static TimeSpan CapRateLimitRetryDelay(TimeSpan delay)
    {
        return delay > MaxRateLimitRetryDelay
            ? MaxRateLimitRetryDelay
            : delay;
    }

    private static AiVisionExtractionDto DeserializeExtraction(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Provider response must be a JSON object.");
        }

        return JsonSerializer.Deserialize<AiVisionExtractionDto>(json, JsonOptions)
            ?? throw new JsonException("Provider response could not be deserialized as AI vision extraction.");
    }

    private AiProviderException CreateProviderException(
        AiProviderRuntimeConfiguration configuration,
        string errorCode,
        string message,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null,
        AiProviderFailureDiagnostic? diagnostic = null)
    {
        return new AiProviderException(
            errorCode,
            ProviderName,
            configuration.ModelName,
            message,
            statusCode,
            innerException,
            diagnostic?.ProviderErrorCode,
            diagnostic?.ProviderErrorType,
            diagnostic?.ProviderErrorMessage,
            diagnostic?.RetryAfter,
            diagnostic?.RequestId,
            diagnostic?.ToJson());
    }

    private AiProviderException CreateHttpStatusException(
        AiProviderRuntimeConfiguration configuration,
        HttpStatusCode statusCode,
        AiProviderFailureDiagnostic diagnostic)
    {
        if (statusCode == HttpStatusCode.Unauthorized)
        {
            return CreateProviderException(configuration, "unauthorized", "AI provider rejected the configured API key.", statusCode, diagnostic: diagnostic);
        }

        if ((int)statusCode == 429)
        {
            return CreateProviderException(configuration, "rate_limited", "AI provider rate limit was exceeded.", statusCode, diagnostic: diagnostic);
        }

        if ((int)statusCode >= 500)
        {
            return CreateProviderException(configuration, "provider_unavailable", "AI provider returned a server error.", statusCode, diagnostic: diagnostic);
        }

        return CreateProviderException(configuration, "http_error", "AI provider returned an unsuccessful HTTP response.", statusCode, diagnostic: diagnostic);
    }

    private void LogProviderFailure(AiProviderException exception)
    {
        var details = exception.DiagnosticJson ??
            $"Provider={exception.ProviderName}; Model={exception.ModelName}; ErrorCode={exception.ErrorCode}; StatusCode={(int?)exception.StatusCode}; ExceptionType={exception.InnerException?.GetType().Name ?? exception.GetType().Name}";
        _logService.ErrorLog(nameof(ExtractSetupAsync), "AI provider extraction failed.", details);
    }

    private void EnsureCompleteExtraction(AiProviderRuntimeConfiguration configuration, AiVisionExtractionDto extraction)
    {
        if (extraction.TriggerId.HasValue &&
            extraction.SceneryId.HasValue &&
            extraction.FigureId.HasValue &&
            extraction.FrameId.HasValue &&
            extraction.StageId.HasValue &&
            extraction.LocationType.HasValue &&
            extraction.ConfirmationType.HasValue &&
            extraction.IsTrendAligned.HasValue &&
            extraction.IsPivotZone.HasValue &&
            extraction.VisualConfidence.HasValue)
        {
            return;
        }

        throw CreateProviderException(configuration, "incomplete_extraction", "AI provider response did not contain a complete vision extraction.");
    }

    private static string ReadContentAsString(JsonElement content)
    {
        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? throw new JsonException("Provider response content was empty.");
        }

        if (content.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString() ?? throw new JsonException("Provider response content was empty.");
                }
            }
        }

        throw new JsonException("Provider response content was not a string.");
    }
}

public sealed record AiVisionImagePayload(
    string OriginalFileName,
    string ContentType,
    long ContentByteLength,
    string DataUri,
    string FrameCode,
    string ImageRole,
    int SortOrder,
    string? Comment);
