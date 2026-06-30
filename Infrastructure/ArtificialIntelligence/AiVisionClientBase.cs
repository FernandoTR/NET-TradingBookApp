using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.DTOs.AiValidation;
using Application.DTOs.AiProviders;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public abstract class AiVisionClientBase : IAiVisionClient
{
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

        try
        {
            if (!string.Equals(configuration.ProviderName, ProviderName, StringComparison.OrdinalIgnoreCase))
            {
                throw CreateProviderException(configuration, "provider_mismatch", $"AI provider configuration '{configuration.ProviderName}' does not match adapter '{ProviderName}'.");
            }

            if (!Uri.TryCreate(configuration.Endpoint, UriKind.Absolute, out var endpoint))
            {
                throw CreateProviderException(configuration, "invalid_endpoint", "AI provider endpoint is not configured with an absolute URL.");
            }

            var imagePayloads = await BuildImagePayloadsAsync(images, cancellationToken);
            var providerRequest = BuildProviderRequest(request, imagePayloads, configuration);
            var requestJson = JsonSerializer.Serialize(providerRequest, JsonOptions);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            var apiKey = Environment.GetEnvironmentVariable(configuration.ApiKeyEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw CreateProviderException(configuration, "missing_api_key", $"Missing API key environment variable '{configuration.ApiKeyEnvironmentVariable}'.");
            }

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(configuration.TimeoutSeconds));

            using var response = await _httpClient.SendAsync(httpRequest, timeoutSource.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw CreateHttpStatusException(configuration, response.StatusCode);
            }

            var responseJson = await response.Content.ReadAsStringAsync(timeoutSource.Token);
            var modelContent = ExtractModelContent(responseJson);
            var extraction = DeserializeExtraction(modelContent);

            EnsureCompleteExtraction(configuration, extraction);

            return extraction;
        }
        catch (AiProviderException ex)
        {
            LogProviderFailure(ex);
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var normalizedException = CreateProviderException(configuration, "timeout", "AI provider request timed out.", innerException: ex);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
        catch (TimeoutException ex)
        {
            var normalizedException = CreateProviderException(configuration, "timeout", "AI provider request timed out.", innerException: ex);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
        catch (HttpRequestException ex)
        {
            var normalizedException = CreateProviderException(configuration, "http_error", "AI provider request failed before a valid response was received.", ex.StatusCode, ex);
            LogProviderFailure(normalizedException);
            throw normalizedException;
        }
        catch (JsonException ex)
        {
            var normalizedException = CreateProviderException(configuration, "invalid_json", "AI provider response was not valid strict JSON.", innerException: ex);
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
                $"data:{image.ContentType};base64,{base64}",
                image.FrameCode,
                image.ImageRole.ToString(),
                image.SortOrder,
                image.Comment));
        }

        return payloads;
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
        Exception? innerException = null)
    {
        return new AiProviderException(errorCode, ProviderName, configuration.ModelName, message, statusCode, innerException);
    }

    private AiProviderException CreateHttpStatusException(AiProviderRuntimeConfiguration configuration, HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.Unauthorized)
        {
            return CreateProviderException(configuration, "unauthorized", "AI provider rejected the configured API key.", statusCode);
        }

        if ((int)statusCode == 429)
        {
            return CreateProviderException(configuration, "rate_limited", "AI provider rate limit was exceeded.", statusCode);
        }

        if ((int)statusCode >= 500)
        {
            return CreateProviderException(configuration, "provider_unavailable", "AI provider returned a server error.", statusCode);
        }

        return CreateProviderException(configuration, "http_error", "AI provider returned an unsuccessful HTTP response.", statusCode);
    }

    private void LogProviderFailure(AiProviderException exception)
    {
        var details = $"Provider={exception.ProviderName}; Model={exception.ModelName}; ErrorCode={exception.ErrorCode}; StatusCode={(int?)exception.StatusCode}; ExceptionType={exception.InnerException?.GetType().Name ?? exception.GetType().Name}";
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
    string DataUri,
    string FrameCode,
    string ImageRole,
    int SortOrder,
    string? Comment);
