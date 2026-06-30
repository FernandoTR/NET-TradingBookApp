using Application.Interfaces;

namespace Application.DTOs.AiProviders;

public sealed class AiVisionClientSelection
{
    public IAiVisionClient Client { get; set; } = null!;

    public AiProviderRuntimeConfiguration Configuration { get; set; } = null!;
}
