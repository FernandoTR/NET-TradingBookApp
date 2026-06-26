using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.ArtificialIntelligence;

public sealed class AiVisionClientFactory : IAiVisionClientFactory
{
    private readonly IReadOnlyCollection<IAiVisionClient> _clients;
    private readonly AiProviderOptions _options;

    public AiVisionClientFactory(IEnumerable<IAiVisionClient> clients, IOptions<AiProviderOptions> options)
    {
        _clients = clients.ToArray();
        _options = options.Value;
    }

    public IAiVisionClient CreateActiveClient()
    {
        if (string.IsNullOrWhiteSpace(_options.ActiveProvider))
        {
            throw new InvalidOperationException("AI active provider is not configured.");
        }

        var provider = ResolveActiveProvider();

        if (!provider.Definition.SupportsVision)
        {
            throw new InvalidOperationException($"AI provider '{provider.Name}' does not support vision and cannot process validation images.");
        }

        ValidateActiveModel(provider.Name, provider.Definition);

        return _clients.FirstOrDefault(client => string.Equals(client.ProviderName, provider.Name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"AI provider '{provider.Name}' is configured but no vision client adapter was registered.");
    }

    private (string Name, AiProviderDefinition Definition) ResolveActiveProvider()
    {
        foreach (var provider in _options.Providers)
        {
            if (string.Equals(provider.Key, _options.ActiveProvider, StringComparison.OrdinalIgnoreCase))
            {
                return (provider.Key, provider.Value);
            }
        }

        var configuredProviders = string.Join(", ", _options.Providers.Keys.OrderBy(provider => provider));
        throw new InvalidOperationException($"AI active provider '{_options.ActiveProvider}' is not configured. Available providers: {configuredProviders}.");
    }

    private void ValidateActiveModel(string providerName, AiProviderDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Model))
        {
            throw new InvalidOperationException($"AI provider '{providerName}' does not define a model.");
        }

        if (string.IsNullOrWhiteSpace(_options.ActiveModel))
        {
            return;
        }

        if (!string.Equals(_options.ActiveModel, definition.Model, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"AI active model '{_options.ActiveModel}' does not match model '{definition.Model}' configured for provider '{providerName}'.");
        }
    }
}
