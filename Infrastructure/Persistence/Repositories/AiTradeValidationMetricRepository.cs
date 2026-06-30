using Application.DTOs.AiValidation;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AiTradeValidationMetricRepository : IAiTradeValidationMetricRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public AiTradeValidationMetricRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public Task<AiTradeValidation?> GetValidationForMetricAsync(int validationId, string userId, CancellationToken cancellationToken)
    {
        return _context.AiTradeValidations
            .AsNoTracking()
            .FirstOrDefaultAsync(validation => validation.Id == validationId && validation.UserId == userId, cancellationToken);
    }

    public Task<AiTradeValidationMetric?> GetByValidationIdAsync(int validationId, CancellationToken cancellationToken)
    {
        return _context.AiTradeValidationMetrics
            .FirstOrDefaultAsync(metric => metric.ValidationId == validationId, cancellationToken);
    }

    public Task<AiTradeValidationMetric?> GetByLinkedOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return _context.AiTradeValidationMetrics
            .Include(metric => metric.Validation)
            .FirstOrDefaultAsync(
                metric => metric.OrderId == orderId || metric.Validation.OrderId == orderId,
                cancellationToken);
    }

    public Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<AiTradeValidationMetric>> GetForSummaryAsync(AiValidationMetricFilterDto filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = _context.AiTradeValidationMetrics.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.ProviderName))
        {
            query = query.Where(metric => metric.ProviderName == filter.ProviderName);
        }

        if (!string.IsNullOrWhiteSpace(filter.ModelName))
        {
            query = query.Where(metric => metric.ModelName == filter.ModelName);
        }

        if (filter.From.HasValue)
        {
            query = query.Where(metric => metric.CreatedAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(metric => metric.CreatedAt <= filter.To.Value);
        }

        return await query
            .OrderByDescending(metric => metric.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AiTradeValidationMetric metric, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(metric);

        try
        {
            await _context.AiTradeValidationMetrics.AddAsync(metric, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(AddAsync), ex);
            throw;
        }
    }

    public async Task UpdateAsync(AiTradeValidationMetric metric, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(metric);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            throw;
        }
    }
}
