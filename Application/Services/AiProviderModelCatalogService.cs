using Application.DTOs.AiProviders;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AiProviderModelCatalogService : IAiProviderModelCatalogService
{
    private readonly IAiProviderModelCatalogRepository _repository;

    public AiProviderModelCatalogService(IAiProviderModelCatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AiProviderModelCatalogDto>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return [];
        }

        var models = await _repository.GetEnabledByProviderAsync(providerName.Trim(), cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<AiProviderModelCatalogDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var model = await _repository.GetByIdAsync(id, cancellationToken);
        return model is null ? null : Map(model);
    }

    private static AiProviderModelCatalogDto Map(AiProviderModelCatalog model)
    {
        return new AiProviderModelCatalogDto
        {
            Id = model.Id,
            ProviderName = model.ProviderName,
            ModelName = model.ModelName,
            ModelId = model.ModelId,
            Endpoint = model.Endpoint,
            ApiProtocol = model.ApiProtocol,
            SupportsVision = model.SupportsVision,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder
        };
    }
}
