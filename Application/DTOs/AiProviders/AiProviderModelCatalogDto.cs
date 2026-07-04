namespace Application.DTOs.AiProviders;

public sealed class AiProviderModelCatalogDto
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string ModelId { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string ApiProtocol { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public bool IsEnabled { get; set; }

    public int SortOrder { get; set; }
}
