using Application.DTOs.AiProviders;
using Application.Interfaces;
using Application.Services;
using Infrastructure;

namespace Application.Tests;

public class AiProviderConfigurationServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenOpenCodeGoCatalogModelExists_DerivesCatalogFields()
    {
        var repository = new FakeAiProviderConfigurationRepository();
        var catalogRepository = new FakeAiProviderModelCatalogRepository(
            CreateCatalogModel(supportsVision: false, apiProtocol: "AnthropicMessages"));
        var service = CreateService(repository, catalogRepository);

        var result = await service.CreateAsync(new AiProviderConfigurationDto
        {
            ModelCatalogId = 10,
            ProviderName = "OpenCodeGo",
            ModelName = "manual-model",
            Endpoint = "https://manual.test/v1/chat/completions",
            ApiProtocol = "OpenAiChatCompletions",
            ApiKeyEnvironmentVariable = "OPENCODE_GO_API_KEY",
            SupportsVision = true,
            TimeoutSeconds = 45,
            IsEnabled = true
        }, "user-test", CancellationToken.None);

        Assert.True(result);

        var provider = Assert.Single(repository.Providers);
        Assert.Equal(10, provider.ModelCatalogId);
        Assert.Equal("OpenCodeGo", provider.ProviderName);
        Assert.Equal("opencode-go-test", provider.ModelName);
        Assert.Equal("https://opencode.test/v1/messages", provider.Endpoint);
        Assert.Equal("AnthropicMessages", provider.ApiProtocol);
        Assert.False(provider.SupportsVision);
        Assert.False(provider.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WhenOpenAiCatalogModelExists_DerivesCatalogFields()
    {
        var repository = new FakeAiProviderConfigurationRepository();
        var catalogRepository = new FakeAiProviderModelCatalogRepository(
            CreateCatalogModel(
                supportsVision: true,
                providerName: "OpenAI",
                modelName: "GPT-4.1 Mini",
                modelId: "gpt-4.1-mini",
                endpoint: "https://api.openai.com/v1/responses"));
        var service = CreateService(repository, catalogRepository);

        var result = await service.CreateAsync(new AiProviderConfigurationDto
        {
            ModelCatalogId = 10,
            ProviderName = "OpenAI",
            ModelName = "manual-model",
            Endpoint = "https://manual.test/v1/chat/completions",
            ApiProtocol = "AnthropicMessages",
            ApiKeyEnvironmentVariable = "OPENAI_API_KEY",
            SupportsVision = false,
            TimeoutSeconds = 60,
            IsEnabled = true
        }, "user-test", CancellationToken.None);

        Assert.True(result);

        var provider = Assert.Single(repository.Providers);
        Assert.Equal(10, provider.ModelCatalogId);
        Assert.Equal("OpenAI", provider.ProviderName);
        Assert.Equal("gpt-4.1-mini", provider.ModelName);
        Assert.Equal("https://api.openai.com/v1/responses", provider.Endpoint);
        Assert.Equal("OpenAiChatCompletions", provider.ApiProtocol);
        Assert.True(provider.SupportsVision);
        Assert.False(provider.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WhenOpenCodeGoCatalogModelIsMissing_ReturnsFalse()
    {
        var repository = new FakeAiProviderConfigurationRepository();
        var service = CreateService(repository, new FakeAiProviderModelCatalogRepository());

        var result = await service.CreateAsync(new AiProviderConfigurationDto
        {
            ModelCatalogId = 999,
            ProviderName = "OpenCodeGo",
            ApiKeyEnvironmentVariable = "OPENCODE_GO_API_KEY",
            TimeoutSeconds = 45,
            IsEnabled = true
        }, "user-test", CancellationToken.None);

        Assert.False(result);
        Assert.Empty(repository.Providers);
    }

    [Fact]
    public async Task CreateAsync_WhenOpenAiCatalogModelIsMissing_ReturnsFalse()
    {
        var repository = new FakeAiProviderConfigurationRepository();
        var service = CreateService(repository, new FakeAiProviderModelCatalogRepository());

        var result = await service.CreateAsync(new AiProviderConfigurationDto
        {
            ModelCatalogId = 999,
            ProviderName = "OpenAI",
            ApiKeyEnvironmentVariable = "OPENAI_API_KEY",
            TimeoutSeconds = 60,
            IsEnabled = true
        }, "user-test", CancellationToken.None);

        Assert.False(result);
        Assert.Empty(repository.Providers);
    }

    [Fact]
    public async Task ActivateAsync_WhenOpenCodeGoModelDoesNotSupportVision_ReturnsFalse()
    {
        var provider = new AiProviderConfiguration
        {
            Id = 1,
            ModelCatalogId = 10,
            ProviderName = "OpenCodeGo",
            ModelName = "opencode-go-test",
            Endpoint = "https://opencode.test/v1/messages",
            ApiProtocol = "AnthropicMessages",
            ApiKeyEnvironmentVariable = "OPENCODE_GO_API_KEY",
            SupportsVision = false,
            TimeoutSeconds = 45,
            IsEnabled = true,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        var repository = new FakeAiProviderConfigurationRepository(provider);
        var service = CreateService(repository, new FakeAiProviderModelCatalogRepository(CreateCatalogModel(supportsVision: false)));

        var result = await service.ActivateAsync(provider.Id, "user-test", CancellationToken.None);

        Assert.False(result);
        Assert.False(provider.IsActive);
        Assert.Empty(repository.UpdatedProviders);
    }

    private static AiProviderConfigurationService CreateService(
        FakeAiProviderConfigurationRepository repository,
        FakeAiProviderModelCatalogRepository catalogRepository)
    {
        return new AiProviderConfigurationService(repository, catalogRepository, new NullLogService());
    }

    private static AiProviderModelCatalog CreateCatalogModel(
        bool supportsVision,
        string apiProtocol = "OpenAiChatCompletions",
        string providerName = "OpenCodeGo",
        string modelName = "OpenCode Go Test",
        string modelId = "opencode-go-test",
        string? endpoint = null)
    {
        return new AiProviderModelCatalog
        {
            Id = 10,
            ProviderName = providerName,
            ModelName = modelName,
            ModelId = modelId,
            Endpoint = endpoint ?? (apiProtocol == "AnthropicMessages"
                ? "https://opencode.test/v1/messages"
                : "https://opencode.test/v1/chat/completions"),
            ApiProtocol = apiProtocol,
            SupportsVision = supportsVision,
            IsEnabled = true,
            SortOrder = 1
        };
    }

    private sealed class FakeAiProviderConfigurationRepository : IAiProviderConfigurationRepository
    {
        public FakeAiProviderConfigurationRepository(params AiProviderConfiguration[] providers)
        {
            Providers = providers.ToList();
        }

        public List<AiProviderConfiguration> Providers { get; }

        public List<AiProviderConfiguration> UpdatedProviders { get; } = [];

        public Task<IReadOnlyList<AiProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AiProviderConfiguration>>(Providers);
        }

        public Task<AiProviderConfiguration?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Providers.FirstOrDefault(provider => provider.Id == id));
        }

        public Task<AiProviderConfiguration?> GetByProviderNameAsync(string providerName, CancellationToken cancellationToken)
        {
            return Task.FromResult(Providers.FirstOrDefault(provider =>
                string.Equals(provider.ProviderName, providerName, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<AiProviderConfiguration?> GetActiveAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Providers.FirstOrDefault(provider => provider.IsActive));
        }

        public Task<bool> AnyAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Providers.Count > 0);
        }

        public Task AddAsync(AiProviderConfiguration provider, CancellationToken cancellationToken)
        {
            provider.Id = provider.Id == 0 ? Providers.Count + 1 : provider.Id;
            Providers.Add(provider);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AiProviderConfiguration provider, CancellationToken cancellationToken)
        {
            UpdatedProviders.Add(provider);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<AiProviderConfiguration> providers, CancellationToken cancellationToken)
        {
            UpdatedProviders.AddRange(providers);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAiProviderModelCatalogRepository : IAiProviderModelCatalogRepository
    {
        private readonly IReadOnlyList<AiProviderModelCatalog> _models;

        public FakeAiProviderModelCatalogRepository(params AiProviderModelCatalog[] models)
        {
            _models = models;
        }

        public Task<IReadOnlyList<AiProviderModelCatalog>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AiProviderModelCatalog>>(_models
                .Where(model => model.IsEnabled && string.Equals(model.ProviderName, providerName, StringComparison.OrdinalIgnoreCase))
                .ToList());
        }

        public Task<AiProviderModelCatalog?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_models.FirstOrDefault(model => model.Id == id));
        }
    }

    private sealed class NullLogService : ILogService
    {
        public void ErrorLog(string methodName, Exception exception)
        {
        }

        public void ErrorLog(string methodName, string message, string details)
        {
        }

        public void ActivityLog(string userId, string eventType, string description)
        {
        }
    }
}
