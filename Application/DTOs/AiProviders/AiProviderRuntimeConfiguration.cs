namespace Application.DTOs.AiProviders;

public sealed class AiProviderRuntimeConfiguration
{
    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; }
}
