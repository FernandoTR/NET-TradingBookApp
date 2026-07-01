using System.Net;

namespace Infrastructure.ArtificialIntelligence;

public sealed class AiProviderException : Exception
{
    public AiProviderException(
        string errorCode,
        string providerName,
        string modelName,
        string message,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null,
        string? providerErrorCode = null,
        string? providerErrorType = null,
        string? providerErrorMessage = null,
        string? retryAfter = null,
        string? requestId = null,
        string? diagnosticJson = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        ProviderName = providerName;
        ModelName = modelName;
        StatusCode = statusCode;
        ProviderErrorCode = providerErrorCode;
        ProviderErrorType = providerErrorType;
        ProviderErrorMessage = providerErrorMessage;
        RetryAfter = retryAfter;
        RequestId = requestId;
        DiagnosticJson = diagnosticJson;
    }

    public string ErrorCode { get; }

    public string ProviderName { get; }

    public string ModelName { get; }

    public HttpStatusCode? StatusCode { get; }

    public string? ProviderErrorCode { get; }

    public string? ProviderErrorType { get; }

    public string? ProviderErrorMessage { get; }

    public string? RetryAfter { get; }

    public string? RequestId { get; }

    public string? DiagnosticJson { get; }
}
