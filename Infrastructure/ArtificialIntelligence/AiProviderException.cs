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
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        ProviderName = providerName;
        ModelName = modelName;
        StatusCode = statusCode;
    }

    public string ErrorCode { get; }

    public string ProviderName { get; }

    public string ModelName { get; }

    public HttpStatusCode? StatusCode { get; }
}
