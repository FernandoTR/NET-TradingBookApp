using Application.DTOs.AiProviders;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public sealed class AiVisionClientFactory : IAiVisionClientFactory
{
    private readonly IReadOnlyCollection<IAiVisionClient> _clients;
    private readonly IAiProviderConfigurationResolver _configurationResolver;

    public AiVisionClientFactory(
        IEnumerable<IAiVisionClient> clients,
        IAiProviderConfigurationResolver configurationResolver)
    {
        _clients = clients.ToArray();
        _configurationResolver = configurationResolver;
    }

    public async Task<AiVisionClientSelection> CreateActiveClientAsync(CancellationToken cancellationToken)
    {
        var configuration = await _configurationResolver.GetActiveAsync(cancellationToken);

        if (!configuration.SupportsVision)
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' does not support vision and cannot process validation images.");
        }

        var client = _clients.FirstOrDefault(client => string.Equals(client.ProviderName, configuration.ProviderName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' is configured but no vision client adapter was registered.");

        return new AiVisionClientSelection
        {
            Client = client,
            Configuration = configuration
        };
    }
}
