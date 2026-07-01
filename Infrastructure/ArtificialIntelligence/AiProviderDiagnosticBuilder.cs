using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Application.DTOs.AiProviders;

namespace Infrastructure.ArtificialIntelligence;

internal static class AiProviderDiagnosticBuilder
{
    private const int MaxBodySnippetLength = 4096;
    private const int MaxHeaderValueLength = 512;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static JsonSerializerOptions JsonOptionsForSerialization => JsonOptions;

    private static readonly Regex DataImageRegex = new(
        @"data:image/[a-zA-Z0-9.+-]+;base64,[A-Za-z0-9+/=]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BearerTokenRegex = new(
        @"Bearer\s+[A-Za-z0-9._~+/=-]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex OpenAiKeyRegex = new(
        @"\bsk-[A-Za-z0-9_\-]{8,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex LongTokenRegex = new(
        @"\b[A-Za-z0-9+/]{120,}={0,2}\b",
        RegexOptions.Compiled);

    public static IReadOnlyList<string?> ExtractImageDetails(string requestJson)
    {
        var details = new List<string?>();

        try
        {
            using var document = JsonDocument.Parse(requestJson);
            ExtractImageDetails(document.RootElement, details);
        }
        catch (JsonException)
        {
            return [];
        }

        return details;
    }

    public static AiProviderFailureDiagnostic CreateHttpStatusDiagnostic(
        string providerName,
        AiProviderRuntimeConfiguration configuration,
        Uri? endpoint,
        HttpResponseMessage response,
        string? responseBody,
        TimeSpan elapsed,
        IReadOnlyList<AiVisionImagePayload> images,
        IReadOnlyList<string?> imageDetails)
    {
        var headers = BuildSafeHeaders(response);
        var providerError = ExtractProviderError(responseBody);

        return new AiProviderFailureDiagnostic
        {
            OccurredAtUtc = DateTime.UtcNow,
            ProviderName = providerName,
            ModelName = configuration.ModelName,
            EndpointHost = endpoint?.Host,
            EndpointPath = endpoint?.AbsolutePath,
            FailureKind = "http_status",
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            ElapsedMilliseconds = elapsed.TotalMilliseconds,
            TimeoutSeconds = configuration.TimeoutSeconds,
            ProviderError = providerError,
            Headers = headers,
            Response = BuildResponseDiagnostic(responseBody),
            ImageSummary = BuildImageSummary(images, imageDetails)
        };
    }

    public static AiProviderFailureDiagnostic CreateExceptionDiagnostic(
        string providerName,
        AiProviderRuntimeConfiguration configuration,
        Uri? endpoint,
        string failureKind,
        Exception exception,
        TimeSpan elapsed,
        IReadOnlyList<AiVisionImagePayload> images,
        IReadOnlyList<string?> imageDetails,
        int? statusCode = null)
    {
        return new AiProviderFailureDiagnostic
        {
            OccurredAtUtc = DateTime.UtcNow,
            ProviderName = providerName,
            ModelName = configuration.ModelName,
            EndpointHost = endpoint?.Host,
            EndpointPath = endpoint?.AbsolutePath,
            FailureKind = failureKind,
            StatusCode = statusCode,
            ElapsedMilliseconds = elapsed.TotalMilliseconds,
            TimeoutSeconds = configuration.TimeoutSeconds,
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = SanitizeProviderText(exception.Message, MaxBodySnippetLength),
            Headers = new SortedDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
            ImageSummary = BuildImageSummary(images, imageDetails)
        };
    }

    private static void ExtractImageDetails(JsonElement element, List<string?> details)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (IsImagePart(element))
            {
                details.Add(ReadOptionalString(element, "detail"));
            }

            foreach (var property in element.EnumerateObject())
            {
                ExtractImageDetails(property.Value, details);
            }

            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractImageDetails(item, details);
            }
        }
    }

    private static bool IsImagePart(JsonElement element)
    {
        if (!element.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var typeName = type.GetString();
        return (string.Equals(typeName, "input_image", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(typeName, "image_url", StringComparison.OrdinalIgnoreCase)) &&
            element.TryGetProperty("image_url", out _);
    }

    private static IReadOnlyDictionary<string, string[]> BuildSafeHeaders(HttpResponseMessage response)
    {
        var headers = new SortedDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        AddSafeHeaders(response.Headers, headers);

        if (response.Content is not null)
        {
            AddSafeHeaders(response.Content.Headers, headers);
        }

        return headers;
    }

    private static void AddSafeHeaders(HttpHeaders source, IDictionary<string, string[]> target)
    {
        foreach (var header in source)
        {
            if (!IsSafeHeader(header.Key))
            {
                continue;
            }

            target[header.Key.ToLowerInvariant()] = header.Value
                .Select(value => Truncate(SanitizeHeaderValue(value), MaxHeaderValueLength))
                .ToArray();
        }
    }

    private static bool IsSafeHeader(string headerName)
    {
        return string.Equals(headerName, "retry-after", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(headerName, "x-request-id", StringComparison.OrdinalIgnoreCase) ||
            headerName.StartsWith("x-ratelimit-", StringComparison.OrdinalIgnoreCase);
    }

    private static ProviderErrorDiagnostic? ExtractProviderError(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var error = root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind == JsonValueKind.Object
                ? errorElement
                : root;

            var message = ReadOptionalString(error, "message");
            var type = ReadOptionalString(error, "type");
            var code = ReadOptionalString(error, "code");
            var param = ReadOptionalString(error, "param");

            return message is null && type is null && code is null && param is null
                ? null
                : new ProviderErrorDiagnostic
                {
                    Message = message,
                    Type = type,
                    Code = code,
                    Param = param
                };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ProviderResponseDiagnostic? BuildResponseDiagnostic(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        var isJson = true;
        try
        {
            using var _ = JsonDocument.Parse(responseBody);
        }
        catch (JsonException)
        {
            isJson = false;
        }

        return new ProviderResponseDiagnostic
        {
            IsJson = isJson,
            BodySnippet = SanitizeProviderText(responseBody, MaxBodySnippetLength)
        };
    }

    private static ImageSummaryDiagnostic BuildImageSummary(
        IReadOnlyList<AiVisionImagePayload> images,
        IReadOnlyList<string?> imageDetails)
    {
        var imageItems = images
            .Select((image, index) => new ImageItemDiagnostic
            {
                ContentType = image.ContentType,
                ContentBytes = image.ContentByteLength,
                FrameCode = image.FrameCode,
                ImageRole = image.ImageRole,
                SortOrder = image.SortOrder,
                Detail = index < imageDetails.Count ? imageDetails[index] : null,
                HasComment = !string.IsNullOrWhiteSpace(image.Comment)
            })
            .ToArray();

        return new ImageSummaryDiagnostic
        {
            Count = images.Count,
            TotalBytes = images.Sum(image => image.ContentByteLength),
            ContentTypes = images
                .Select(image => image.ContentType)
                .Where(contentType => !string.IsNullOrWhiteSpace(contentType))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(contentType => contentType)
                .ToArray(),
            SortOrders = images.Select(image => image.SortOrder).ToArray(),
            DetailValues = imageDetails
                .Where(detail => !string.IsNullOrWhiteSpace(detail))
                .Select(detail => detail!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(detail => detail)
                .ToArray(),
            Images = imageItems
        };
    }

    private static string? ReadOptionalString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var value = property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.GetRawText();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : SanitizeProviderText(value, MaxBodySnippetLength);
    }

    private static string SanitizeHeaderValue(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Truncate(value.Trim(), MaxHeaderValueLength);
    }

    private static string SanitizeProviderText(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sanitized = DataImageRegex.Replace(value, "[redacted-data-uri]");
        sanitized = BearerTokenRegex.Replace(sanitized, "Bearer [redacted]");
        sanitized = OpenAiKeyRegex.Replace(sanitized, "[redacted-api-key]");
        sanitized = LongTokenRegex.Replace(sanitized, "[redacted-long-token]");

        return Truncate(sanitized, maxLength);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..maxLength]}...[truncated]";
    }
}

internal sealed class AiProviderFailureDiagnostic
{
    public DateTime OccurredAtUtc { get; init; }

    public string ProviderName { get; init; } = null!;

    public string ModelName { get; init; } = null!;

    public string? EndpointHost { get; init; }

    public string? EndpointPath { get; init; }

    public string FailureKind { get; init; } = null!;

    public int? StatusCode { get; init; }

    public string? ReasonPhrase { get; init; }

    public double ElapsedMilliseconds { get; init; }

    public int TimeoutSeconds { get; init; }

    public string? ExceptionType { get; init; }

    public string? ExceptionMessage { get; init; }

    public ProviderErrorDiagnostic? ProviderError { get; init; }

    public IReadOnlyDictionary<string, string[]> Headers { get; init; } =
        new SortedDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public ProviderResponseDiagnostic? Response { get; init; }

    public ImageSummaryDiagnostic ImageSummary { get; init; } = new();

    [JsonIgnore]
    public string? ProviderErrorCode => ProviderError?.Code;

    [JsonIgnore]
    public string? ProviderErrorType => ProviderError?.Type;

    [JsonIgnore]
    public string? ProviderErrorMessage => ProviderError?.Message;

    [JsonIgnore]
    public string? RetryAfter => GetFirstHeaderValue("retry-after");

    [JsonIgnore]
    public string? RequestId => GetFirstHeaderValue("x-request-id");

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, AiProviderDiagnosticBuilder.JsonOptionsForSerialization);
    }

    private string? GetFirstHeaderValue(string headerName)
    {
        return Headers.TryGetValue(headerName, out var values)
            ? values.FirstOrDefault()
            : null;
    }
}

internal sealed class ProviderErrorDiagnostic
{
    public string? Message { get; init; }

    public string? Type { get; init; }

    public string? Code { get; init; }

    public string? Param { get; init; }
}

internal sealed class ProviderResponseDiagnostic
{
    public bool IsJson { get; init; }

    public string BodySnippet { get; init; } = string.Empty;
}

internal sealed class ImageSummaryDiagnostic
{
    public int Count { get; init; }

    public long TotalBytes { get; init; }

    public IReadOnlyList<string> ContentTypes { get; init; } = [];

    public IReadOnlyList<int> SortOrders { get; init; } = [];

    public IReadOnlyList<string> DetailValues { get; init; } = [];

    public IReadOnlyList<ImageItemDiagnostic> Images { get; init; } = [];
}

internal sealed class ImageItemDiagnostic
{
    public string ContentType { get; init; } = string.Empty;

    public long ContentBytes { get; init; }

    public string FrameCode { get; init; } = string.Empty;

    public string ImageRole { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public string? Detail { get; init; }

    public bool HasComment { get; init; }
}
