using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AiTradeValidationRepository : IAiTradeValidationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public AiTradeValidationRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<int> SaveCompletedAsync(AiTradeValidation validation, IEnumerable<AiTradeValidationRule> rules, CancellationToken cancellationToken)
    {
        ValidateCompletedValidation(validation);

        var ruleList = rules.ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            validation.Rules.Clear();

            await _context.AiTradeValidations.AddAsync(validation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var rule in ruleList)
            {
                rule.ValidationId = validation.Id;
            }

            await _context.AiTradeValidationRules.AddRangeAsync(ruleList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return validation.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logService.ErrorLog(nameof(SaveCompletedAsync), ex);
            throw;
        }
    }

    public Task<AiTradeValidation?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken)
    {
        return _context.AiTradeValidations
            .Include(validation => validation.Rules)
            .AsNoTracking()
            .FirstOrDefaultAsync(validation => validation.Id == id && validation.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<AiTradeValidation>> GetByUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.AiTradeValidations
            .Include(validation => validation.Rules)
            .AsNoTracking()
            .Where(validation => validation.UserId == userId)
            .OrderByDescending(validation => validation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> LinkOrderAsync(int validationId, int orderId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var validation = await _context.AiTradeValidations
                .FirstOrDefaultAsync(item => item.Id == validationId && item.UserId == userId, cancellationToken);

            if (validation == null)
            {
                return false;
            }

            validation.OrderId = orderId;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(LinkOrderAsync), ex);
            throw;
        }
    }

    private static void ValidateCompletedValidation(AiTradeValidation validation)
    {
        ArgumentNullException.ThrowIfNull(validation);

        if (string.IsNullOrWhiteSpace(validation.ModelResponseJson))
        {
            throw new ArgumentException("ModelResponseJson is required to save a completed AI trade validation.", nameof(validation));
        }

        if (string.IsNullOrWhiteSpace(validation.ProviderName))
        {
            throw new ArgumentException("ProviderName is required to save a completed AI trade validation.", nameof(validation));
        }

        if (string.IsNullOrWhiteSpace(validation.ModelName))
        {
            throw new ArgumentException("ModelName is required to save a completed AI trade validation.", nameof(validation));
        }

        if (string.IsNullOrWhiteSpace(validation.FinalSummary))
        {
            throw new ArgumentException("FinalSummary is required to save a completed AI trade validation.", nameof(validation));
        }
    }
}
