using Application.DTOs.AiProviders;

namespace Application.Interfaces;

public interface IAiProviderConfigurationService
{
    Task<IReadOnlyList<AiProviderConfigurationDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AiProviderConfigurationDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> CreateAsync(AiProviderConfigurationDto provider, string userId, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(AiProviderConfigurationDto provider, string userId, CancellationToken cancellationToken);

    Task<bool> ActivateAsync(int id, string userId, CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(int id, string userId, CancellationToken cancellationToken);
}
