using Application.DTOs.AiProviders;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AiProviderConfigurationService : IAiProviderConfigurationService
{
    private const string OpenAiProviderName = "OpenAI";
    private const string OpenCodeGoProviderName = "OpenCodeGo";
    private const string DefaultApiProtocol = "OpenAiChatCompletions";

    private static readonly string[] SupportedProviders =
    [
        OpenAiProviderName,
        "MiniMax",
        "DeepSeek",
        "GLM",
        "Kimi",
        OpenCodeGoProviderName
    ];

    private static readonly string[] CatalogBackedProviders =
    [
        OpenAiProviderName,
        OpenCodeGoProviderName
    ];

    private static readonly string[] SupportedApiProtocols =
    [
        "OpenAiChatCompletions",
        "AnthropicMessages"
    ];

    private readonly IAiProviderConfigurationRepository _repository;
    private readonly IAiProviderModelCatalogRepository _modelCatalogRepository;
    private readonly ILogService _logService;

    public AiProviderConfigurationService(
        IAiProviderConfigurationRepository repository,
        IAiProviderModelCatalogRepository modelCatalogRepository,
        ILogService logService)
    {
        _repository = repository;
        _modelCatalogRepository = modelCatalogRepository;
        _logService = logService;
    }

    public async Task<IReadOnlyList<AiProviderConfigurationDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var providers = await _repository.GetAllAsync(cancellationToken);
        return providers.Select(Map).ToList();
    }

    public async Task<AiProviderConfigurationDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var provider = await _repository.GetByIdAsync(id, cancellationToken);
        return provider is null ? null : Map(provider);
    }

    public async Task<bool> CreateAsync(
        AiProviderConfigurationDto provider,
        string userId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);
        EnsureUserId(userId);

        if (!IsValidForSave(provider))
        {
            return false;
        }

        var normalizedProviderName = provider.ProviderName.Trim();
        var existing = await _repository.GetByProviderNameAsync(normalizedProviderName, cancellationToken);
        if (existing is not null)
        {
            return false;
        }

        var catalogModel = await ResolveCatalogModelAsync(provider, normalizedProviderName, cancellationToken);
        if (IsCatalogBackedProvider(normalizedProviderName) && catalogModel is null)
        {
            return false;
        }

        var candidate = BuildSaveCandidate(provider, normalizedProviderName, catalogModel);

        try
        {
            var entity = new AiProviderConfiguration
            {
                ModelCatalogId = candidate.ModelCatalogId,
                ProviderName = candidate.ProviderName,
                ModelName = candidate.ModelName,
                Endpoint = candidate.Endpoint,
                ApiProtocol = candidate.ApiProtocol,
                ApiKeyEnvironmentVariable = candidate.ApiKeyEnvironmentVariable,
                SupportsVision = candidate.SupportsVision,
                TimeoutSeconds = candidate.TimeoutSeconds,
                IsActive = false,
                IsEnabled = candidate.IsEnabled,
                CreatedAt = DateTime.UtcNow
            };

            if (!entity.IsEnabled)
            {
                entity.DeactivatedAt = DateTime.UtcNow;
            }

            await _repository.AddAsync(entity, cancellationToken);
            _logService.ActivityLog(userId, "Creacion de proveedor IA", $"Proveedor IA creado: {entity.ProviderName}; Modelo: {entity.ModelName}");

            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(CreateAsync), ex);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(
        AiProviderConfigurationDto provider,
        string userId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);
        EnsureUserId(userId);

        if (provider.Id <= 0 || !IsValidForSave(provider))
        {
            return false;
        }

        try
        {
            var entity = await _repository.GetByIdAsync(provider.Id, cancellationToken);
            if (entity is null)
            {
                return false;
            }

            var normalizedProviderName = provider.ProviderName.Trim();
            var duplicated = await _repository.GetByProviderNameAsync(normalizedProviderName, cancellationToken);
            if (duplicated is not null && duplicated.Id != entity.Id)
            {
                return false;
            }

            var catalogModel = await ResolveCatalogModelAsync(provider, normalizedProviderName, cancellationToken);
            if (IsCatalogBackedProvider(normalizedProviderName) && catalogModel is null)
            {
                return false;
            }

            var candidate = BuildSaveCandidate(provider, normalizedProviderName, catalogModel);

            if (entity.IsActive && !CanActivate(candidate))
            {
                return false;
            }

            entity.ModelCatalogId = candidate.ModelCatalogId;
            entity.ProviderName = candidate.ProviderName;
            entity.ModelName = candidate.ModelName;
            entity.Endpoint = candidate.Endpoint;
            entity.ApiProtocol = candidate.ApiProtocol;
            entity.ApiKeyEnvironmentVariable = candidate.ApiKeyEnvironmentVariable;
            entity.SupportsVision = candidate.SupportsVision;
            entity.TimeoutSeconds = candidate.TimeoutSeconds;
            entity.IsEnabled = candidate.IsEnabled;
            entity.UpdatedAt = DateTime.UtcNow;

            if (!entity.IsEnabled)
            {
                entity.IsActive = false;
                entity.DeactivatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.DeactivatedAt = null;
            }

            await _repository.UpdateAsync(entity, cancellationToken);
            _logService.ActivityLog(userId, "Actualizacion de proveedor IA", $"Proveedor IA actualizado: {entity.ProviderName}; Modelo: {entity.ModelName}");

            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            throw;
        }
    }

    public async Task<bool> ActivateAsync(int id, string userId, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        EnsureUserId(userId);

        try
        {
            var providers = await _repository.GetAllAsync(cancellationToken);
            var target = providers.FirstOrDefault(provider => provider.Id == id);
            if (target is null || !CanActivate(Map(target)))
            {
                return false;
            }

            var changedProviders = providers
                .Where(provider => provider.IsActive)
                .ToList();

            foreach (var provider in changedProviders)
            {
                provider.IsActive = false;
                provider.UpdatedAt = DateTime.UtcNow;
            }

            if (changedProviders.Count > 0)
            {
                await _repository.UpdateRangeAsync(changedProviders, cancellationToken);
            }

            target.IsActive = true;
            target.IsEnabled = true;
            target.DeactivatedAt = null;
            target.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(target, cancellationToken);
            _logService.ActivityLog(userId, "Activacion de proveedor IA", $"Proveedor IA activo: {target.ProviderName}; Modelo: {target.ModelName}");

            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(ActivateAsync), ex);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(int id, string userId, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        EnsureUserId(userId);

        try
        {
            var provider = await _repository.GetByIdAsync(id, cancellationToken);
            if (provider is null)
            {
                return false;
            }

            provider.IsActive = false;
            provider.IsEnabled = false;
            provider.DeactivatedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(provider, cancellationToken);
            _logService.ActivityLog(userId, "Desactivacion de proveedor IA", $"Proveedor IA desactivado: {provider.ProviderName}; Modelo: {provider.ModelName}");

            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(DeactivateAsync), ex);
            throw;
        }
    }

    private static AiProviderConfigurationDto Map(AiProviderConfiguration provider)
    {
        return new AiProviderConfigurationDto
        {
            Id = provider.Id,
            ModelCatalogId = provider.ModelCatalogId,
            ProviderName = provider.ProviderName,
            ModelName = provider.ModelName,
            Endpoint = provider.Endpoint,
            ApiProtocol = provider.ApiProtocol,
            ApiKeyEnvironmentVariable = provider.ApiKeyEnvironmentVariable,
            IsApiKeyConfigured = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(provider.ApiKeyEnvironmentVariable)),
            SupportsVision = provider.SupportsVision,
            TimeoutSeconds = provider.TimeoutSeconds,
            IsActive = provider.IsActive,
            IsEnabled = provider.IsEnabled
        };
    }

    private static bool IsValidForSave(AiProviderConfigurationDto provider)
    {
        if (!IsSupportedProvider(provider.ProviderName) ||
            string.IsNullOrWhiteSpace(provider.ApiKeyEnvironmentVariable) ||
            provider.TimeoutSeconds <= 0)
        {
            return false;
        }

        if (IsCatalogBackedProvider(provider.ProviderName))
        {
            return provider.ModelCatalogId.GetValueOrDefault() > 0;
        }

        return !string.IsNullOrWhiteSpace(provider.ModelName) &&
            IsValidApiProtocol(provider.ApiProtocol);
    }

    private static bool CanActivate(AiProviderConfigurationDto provider)
    {
        return provider.IsEnabled &&
            provider.SupportsVision &&
            IsValidForSave(provider) &&
            Uri.TryCreate(provider.Endpoint, UriKind.Absolute, out _);
    }

    private static bool IsSupportedProvider(string providerName)
    {
        return SupportedProviders.Any(provider =>
            string.Equals(provider, providerName?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private async Task<AiProviderModelCatalog?> ResolveCatalogModelAsync(
        AiProviderConfigurationDto provider,
        string normalizedProviderName,
        CancellationToken cancellationToken)
    {
        if (!IsCatalogBackedProvider(normalizedProviderName) || provider.ModelCatalogId.GetValueOrDefault() <= 0)
        {
            return null;
        }

        var catalogModel = await _modelCatalogRepository.GetByIdAsync(provider.ModelCatalogId!.Value, cancellationToken);
        if (catalogModel is null ||
            !catalogModel.IsEnabled ||
            !string.Equals(catalogModel.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return catalogModel;
    }

    private static AiProviderConfigurationDto BuildSaveCandidate(
        AiProviderConfigurationDto provider,
        string normalizedProviderName,
        AiProviderModelCatalog? catalogModel)
    {
        return new AiProviderConfigurationDto
        {
            Id = provider.Id,
            ModelCatalogId = catalogModel?.Id,
            ProviderName = normalizedProviderName,
            ModelName = catalogModel?.ModelId ?? provider.ModelName.Trim(),
            Endpoint = catalogModel?.Endpoint ?? NormalizeEndpoint(provider.Endpoint),
            ApiProtocol = catalogModel?.ApiProtocol ?? NormalizeApiProtocol(provider.ApiProtocol),
            ApiKeyEnvironmentVariable = provider.ApiKeyEnvironmentVariable.Trim(),
            IsApiKeyConfigured = provider.IsApiKeyConfigured,
            SupportsVision = catalogModel?.SupportsVision ?? provider.SupportsVision,
            TimeoutSeconds = provider.TimeoutSeconds,
            IsActive = provider.IsActive,
            IsEnabled = provider.IsEnabled
        };
    }

    private static bool IsCatalogBackedProvider(string? providerName)
    {
        return CatalogBackedProviders.Any(provider =>
            string.Equals(provider, providerName?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidApiProtocol(string? apiProtocol)
    {
        return string.IsNullOrWhiteSpace(apiProtocol) ||
            SupportedApiProtocols.Any(protocol => string.Equals(protocol, apiProtocol.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeApiProtocol(string? apiProtocol)
    {
        return string.IsNullOrWhiteSpace(apiProtocol) ? DefaultApiProtocol : apiProtocol.Trim();
    }

    private static string? NormalizeEndpoint(string? endpoint)
    {
        return string.IsNullOrWhiteSpace(endpoint) ? null : endpoint.Trim();
    }

    private static void EnsureUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId is required to manage AI providers.", nameof(userId));
        }
    }
}
