using Application.DTOs.AiProviders;

namespace Application.Interfaces;

public interface IAiProviderConfigurationResolver
{
    Task<AiProviderRuntimeConfiguration> GetActiveAsync(CancellationToken cancellationToken);
}
