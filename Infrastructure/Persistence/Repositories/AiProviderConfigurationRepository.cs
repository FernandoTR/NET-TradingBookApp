using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AiProviderConfigurationRepository : IAiProviderConfigurationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public AiProviderConfigurationRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IReadOnlyList<AiProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.AiProviderConfigurations
            .OrderByDescending(provider => provider.IsActive)
            .ThenBy(provider => provider.ProviderName)
            .ToListAsync(cancellationToken);
    }

    public Task<AiProviderConfiguration?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.AiProviderConfigurations
            .FirstOrDefaultAsync(provider => provider.Id == id, cancellationToken);
    }

    public Task<AiProviderConfiguration?> GetByProviderNameAsync(string providerName, CancellationToken cancellationToken)
    {
        return _context.AiProviderConfigurations
            .FirstOrDefaultAsync(provider => provider.ProviderName == providerName, cancellationToken);
    }

    public Task<AiProviderConfiguration?> GetActiveAsync(CancellationToken cancellationToken)
    {
        return _context.AiProviderConfigurations
            .FirstOrDefaultAsync(provider => provider.IsActive && provider.IsEnabled, cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken)
    {
        return _context.AiProviderConfigurations.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(AiProviderConfiguration provider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);

        try
        {
            await _context.AiProviderConfigurations.AddAsync(provider, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(AddAsync), ex);
            throw;
        }
    }

    public async Task UpdateAsync(AiProviderConfiguration provider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);

        try
        {
            _context.AiProviderConfigurations.Update(provider);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            throw;
        }
    }

    public async Task UpdateRangeAsync(IEnumerable<AiProviderConfiguration> providers, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(providers);

        try
        {
            _context.AiProviderConfigurations.UpdateRange(providers);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateRangeAsync), ex);
            throw;
        }
    }
}
