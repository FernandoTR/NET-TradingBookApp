using Infrastructure;

namespace Application.Interfaces;

public interface IAiProviderConfigurationRepository
{
    Task<IReadOnlyList<AiProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken);

    Task<AiProviderConfiguration?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<AiProviderConfiguration?> GetByProviderNameAsync(string providerName, CancellationToken cancellationToken);

    Task<AiProviderConfiguration?> GetActiveAsync(CancellationToken cancellationToken);

    Task<bool> AnyAsync(CancellationToken cancellationToken);

    Task AddAsync(AiProviderConfiguration provider, CancellationToken cancellationToken);

    Task UpdateAsync(AiProviderConfiguration provider, CancellationToken cancellationToken);

    Task UpdateRangeAsync(IEnumerable<AiProviderConfiguration> providers, CancellationToken cancellationToken);
}
