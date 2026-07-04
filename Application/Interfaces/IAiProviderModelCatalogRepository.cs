using Infrastructure;

namespace Application.Interfaces;

public interface IAiProviderModelCatalogRepository
{
    Task<IReadOnlyList<AiProviderModelCatalog>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken);

    Task<AiProviderModelCatalog?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
