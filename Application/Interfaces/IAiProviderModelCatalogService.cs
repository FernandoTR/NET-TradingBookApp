using Application.DTOs.AiProviders;

namespace Application.Interfaces;

public interface IAiProviderModelCatalogService
{
    Task<IReadOnlyList<AiProviderModelCatalogDto>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken);

    Task<AiProviderModelCatalogDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
