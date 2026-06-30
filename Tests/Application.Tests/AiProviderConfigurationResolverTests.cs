using Application.Common;
using Application.Interfaces;
using Application.Services;
using Infrastructure;
using Microsoft.Extensions.Options;

namespace Application.Tests;

public class AiProviderConfigurationResolverTests
{
    [Fact]
    public async Task GetActiveAsync_WhenSqlHasProvidersWithoutActive_DoesNotUseFallback()
    {
        var fallbackApiKeyName = $"{nameof(GetActiveAsync_WhenSqlHasProvidersWithoutActive_DoesNotUseFallback)}_KEY";
        Environment.SetEnvironmentVariable(fallbackApiKeyName, "test-key");

        try
        {
            var repository = new FakeAiProviderConfigurationRepository(
                hasProviders: true,
                activeProvider: null);

            var resolver = CreateResolver(repository, fallbackApiKeyName);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                resolver.GetActiveAsync(CancellationToken.None));

            Assert.Contains("No active AI provider", exception.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable(fallbackApiKeyName, null);
        }
    }

    [Fact]
    public async Task GetActiveAsync_WhenActiveSqlProviderIsDisabled_Throws()
    {
        var apiKeyName = $"{nameof(GetActiveAsync_WhenActiveSqlProviderIsDisabled_Throws)}_KEY";
        Environment.SetEnvironmentVariable(apiKeyName, "test-key");

        try
        {
            var repository = new FakeAiProviderConfigurationRepository(
                hasProviders: true,
                activeProvider: CreateProvider(apiKeyName, isEnabled: false));

            var resolver = CreateResolver(repository, apiKeyName);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                resolver.GetActiveAsync(CancellationToken.None));

            Assert.Contains("disabled", exception.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable(apiKeyName, null);
        }
    }

    [Fact]
    public async Task GetActiveAsync_WhenSqlHasNoProviders_UsesFallbackOptions()
    {
        var apiKeyName = $"{nameof(GetActiveAsync_WhenSqlHasNoProviders_UsesFallbackOptions)}_KEY";
        Environment.SetEnvironmentVariable(apiKeyName, "test-key");

        try
        {
            var repository = new FakeAiProviderConfigurationRepository(
                hasProviders: false,
                activeProvider: null);

            var resolver = CreateResolver(repository, apiKeyName);
            var configuration = await resolver.GetActiveAsync(CancellationToken.None);

            Assert.Equal("OpenAI", configuration.ProviderName);
            Assert.Equal("gpt-test", configuration.ModelName);
            Assert.Equal("https://api.test.local/v1/responses", configuration.Endpoint);
            Assert.Equal(apiKeyName, configuration.ApiKeyEnvironmentVariable);
        }
        finally
        {
            Environment.SetEnvironmentVariable(apiKeyName, null);
        }
    }

    private static AiProviderConfigurationResolver CreateResolver(
        IAiProviderConfigurationRepository repository,
        string apiKeyEnvironmentVariable)
    {
        return new AiProviderConfigurationResolver(
            repository,
            Options.Create(new AiProviderOptions
            {
                ActiveProvider = "OpenAI",
                ActiveModel = "gpt-test",
                Providers = new Dictionary<string, AiProviderDefinition>
                {
                    ["OpenAI"] = new()
                    {
                        Model = "gpt-test",
                        Endpoint = "https://api.test.local/v1/responses",
                        ApiKeyEnvironmentVariable = apiKeyEnvironmentVariable,
                        SupportsVision = true,
                        TimeoutSeconds = 30
                    }
                }
            }));
    }

    private static AiProviderConfiguration CreateProvider(string apiKeyEnvironmentVariable, bool isEnabled)
    {
        return new AiProviderConfiguration
        {
            Id = 1,
            ProviderName = "OpenAI",
            ModelName = "gpt-test",
            Endpoint = "https://api.test.local/v1/responses",
            ApiKeyEnvironmentVariable = apiKeyEnvironmentVariable,
            SupportsVision = true,
            TimeoutSeconds = 30,
            IsActive = true,
            IsEnabled = isEnabled,
            CreatedAt = DateTime.UtcNow
        };
    }

    private sealed class FakeAiProviderConfigurationRepository : IAiProviderConfigurationRepository
    {
        private readonly bool _hasProviders;
        private readonly AiProviderConfiguration? _activeProvider;

        public FakeAiProviderConfigurationRepository(
            bool hasProviders,
            AiProviderConfiguration? activeProvider)
        {
            _hasProviders = hasProviders;
            _activeProvider = activeProvider;
        }

        public Task<IReadOnlyList<AiProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<AiProviderConfiguration> providers = _activeProvider is null
                ? []
                : [_activeProvider];

            return Task.FromResult(providers);
        }

        public Task<AiProviderConfiguration?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_activeProvider?.Id == id ? _activeProvider : null);
        }

        public Task<AiProviderConfiguration?> GetByProviderNameAsync(string providerName, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                string.Equals(_activeProvider?.ProviderName, providerName, StringComparison.OrdinalIgnoreCase)
                    ? _activeProvider
                    : null);
        }

        public Task<AiProviderConfiguration?> GetActiveAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_activeProvider);
        }

        public Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_hasProviders);
        }

        public Task AddAsync(AiProviderConfiguration provider, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(AiProviderConfiguration provider, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task UpdateRangeAsync(IEnumerable<AiProviderConfiguration> providers, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
