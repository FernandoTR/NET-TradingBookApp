namespace Infrastructure;

public partial class AiProviderModelCatalog
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string ModelId { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string ApiProtocol { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }
}
