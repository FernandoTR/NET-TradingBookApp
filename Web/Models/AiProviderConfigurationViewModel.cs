using System.ComponentModel;

namespace Web.Models;

public class AiProviderConfigurationViewModel
{
    public int Id { get; set; }

    public int? ModelCatalogId { get; set; }

    [DisplayName("Proveedor")]
    public string ProviderName { get; set; } = string.Empty;

    [DisplayName("Modelo")]
    public string ModelName { get; set; } = string.Empty;

    [DisplayName("Endpoint")]
    public string? Endpoint { get; set; }

    [DisplayName("Protocolo API")]
    public string ApiProtocol { get; set; } = string.Empty;

    [DisplayName("Variable/secret API key")]
    public string ApiKeyEnvironmentVariable { get; set; } = string.Empty;

    public bool IsApiKeyConfigured { get; set; }

    [DisplayName("Vision")]
    public bool SupportsVision { get; set; }

    [DisplayName("Timeout")]
    public int TimeoutSeconds { get; set; } = 60;

    public bool IsActive { get; set; }

    [DisplayName("Habilitado")]
    public bool IsEnabled { get; set; } = true;

    public string ApiKeyStatus { get; set; } = string.Empty;

    public string VisionStatus { get; set; } = string.Empty;

    public string ActiveStatus { get; set; } = string.Empty;

    public string EnabledStatus { get; set; } = string.Empty;

    public string Task { get; set; } = string.Empty;
}
