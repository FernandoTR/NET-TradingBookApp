using Application.Common;
using Application.DTOs.AiProviders;
using Application.Interfaces;
using Infrastructure;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AiProviderConfigurationResolver : IAiProviderConfigurationResolver
{
    private const string DefaultApiProtocol = "OpenAiChatCompletions";

    private static readonly string[] SupportedApiProtocols =
    [
        "OpenAiChatCompletions",
        "AnthropicMessages"
    ];

    private readonly IAiProviderConfigurationRepository _repository;
    private readonly AiProviderOptions _options;

    public AiProviderConfigurationResolver(
        IAiProviderConfigurationRepository repository,
        IOptions<AiProviderOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<AiProviderRuntimeConfiguration> GetActiveAsync(CancellationToken cancellationToken)
    {
        var hasConfiguredProviders = await _repository.AnyAsync(cancellationToken);
        if (hasConfiguredProviders)
        {
            var activeProvider = await _repository.GetActiveAsync(cancellationToken);
            if (activeProvider is null)
            {
                throw new InvalidOperationException("No active AI provider is configured in SQL.");
            }

            if (!activeProvider.IsEnabled)
            {
                throw new InvalidOperationException($"AI provider '{activeProvider.ProviderName}' is disabled and cannot be used.");
            }

            var databaseConfiguration = MapFromEntity(activeProvider);
            ValidateRuntimeConfiguration(databaseConfiguration);
            return databaseConfiguration;
        }

        var fallbackConfiguration = ResolveFromOptions();
        ValidateRuntimeConfiguration(fallbackConfiguration);
        return fallbackConfiguration;
    }

    private AiProviderRuntimeConfiguration ResolveFromOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ActiveProvider))
        {
            throw new InvalidOperationException("AI active provider is not configured.");
        }

        var provider = ResolveActiveProviderDefinition();
        ValidateActiveModel(provider.Name, provider.Definition);

        return new AiProviderRuntimeConfiguration
        {
            ProviderName = provider.Name,
            ModelName = provider.Definition.Model,
            Endpoint = provider.Definition.Endpoint ?? string.Empty,
            ApiProtocol = DefaultApiProtocol,
            ApiKeyEnvironmentVariable = provider.Definition.ApiKeyEnvironmentVariable,
            SupportsVision = provider.Definition.SupportsVision,
            TimeoutSeconds = provider.Definition.TimeoutSeconds
        };
    }

    private (string Name, AiProviderDefinition Definition) ResolveActiveProviderDefinition()
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

    private static AiProviderRuntimeConfiguration MapFromEntity(AiProviderConfiguration provider)
    {
        return new AiProviderRuntimeConfiguration
        {
            ProviderName = provider.ProviderName,
            ModelName = provider.ModelName,
            Endpoint = provider.Endpoint ?? string.Empty,
            ApiProtocol = NormalizeApiProtocol(provider.ApiProtocol),
            ApiKeyEnvironmentVariable = provider.ApiKeyEnvironmentVariable,
            SupportsVision = provider.SupportsVision,
            TimeoutSeconds = provider.TimeoutSeconds
        };
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

    private static void ValidateRuntimeConfiguration(AiProviderRuntimeConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.ProviderName))
        {
            throw new InvalidOperationException("AI active provider is not configured.");
        }

        if (string.IsNullOrWhiteSpace(configuration.ModelName))
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' does not define a model.");
        }

        if (!configuration.SupportsVision)
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' does not support vision and cannot process validation images.");
        }

        if (!Uri.TryCreate(configuration.Endpoint, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' does not define a valid absolute endpoint.");
        }

        if (!IsValidApiProtocol(configuration.ApiProtocol))
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' defines unsupported API protocol '{configuration.ApiProtocol}'.");
        }

        if (string.IsNullOrWhiteSpace(configuration.ApiKeyEnvironmentVariable))
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' does not define an API key environment variable.");
        }

        if (configuration.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' must define a timeout greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(configuration.ApiKeyEnvironmentVariable)))
        {
            throw new InvalidOperationException($"AI provider '{configuration.ProviderName}' API key environment variable '{configuration.ApiKeyEnvironmentVariable}' is not configured.");
        }
    }

    private static string NormalizeApiProtocol(string? apiProtocol)
    {
        return string.IsNullOrWhiteSpace(apiProtocol) ? DefaultApiProtocol : apiProtocol.Trim();
    }

    private static bool IsValidApiProtocol(string? apiProtocol)
    {
        return SupportedApiProtocols.Any(protocol => string.Equals(protocol, apiProtocol?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
