using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AiProviderModelCatalogRepository : IAiProviderModelCatalogRepository
{
    private readonly ApplicationDbContext _context;

    public AiProviderModelCatalogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AiProviderModelCatalog>> GetEnabledByProviderAsync(string providerName, CancellationToken cancellationToken)
    {
        return await _context.AiProviderModelCatalogs
            .AsNoTracking()
            .Where(model => model.ProviderName == providerName && model.IsEnabled)
            .OrderBy(model => model.SortOrder)
            .ThenBy(model => model.ModelName)
            .ToListAsync(cancellationToken);
    }

    public Task<AiProviderModelCatalog?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.AiProviderModelCatalogs
            .AsNoTracking()
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }
}
