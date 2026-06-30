namespace Application.DTOs.AiProviders;

public sealed class AiProviderConfigurationDto
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string? Endpoint { get; set; }

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool IsApiKeyConfigured { get; set; }

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; }

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; }
}
