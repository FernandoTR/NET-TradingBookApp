using Application.DTOs.AiValidation;
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

    public async Task<IReadOnlyList<AiTradeValidation>> GetCompletedByUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.AiTradeValidations
            .AsNoTracking()
            .Where(validation =>
                validation.UserId == userId &&
                validation.ModelResponseJson != "" &&
                validation.ProviderName != "" &&
                validation.ModelName != "" &&
                validation.FinalSummary != "" &&
                validation.DetectedTriggerId.HasValue &&
                validation.DetectedSceneryId.HasValue &&
                validation.DetectedFigureId.HasValue &&
                validation.DetectedFrameId.HasValue &&
                validation.DetectedStageId.HasValue &&
                validation.DetectedLocationType.HasValue &&
                validation.DetectedConfirmationType.HasValue &&
                validation.DetectedIsTrendAligned.HasValue &&
                validation.DetectedIsPivotZone.HasValue &&
                validation.VisualConfidence.HasValue)
            .OrderByDescending(validation => validation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ConfirmAsync(ConfirmedAiValidationDto confirmation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(confirmation);

        try
        {
            var validation = await _context.AiTradeValidations
                .FirstOrDefaultAsync(item => item.Id == confirmation.ValidationId && item.UserId == confirmation.UserId, cancellationToken);

            if (validation is null)
            {
                return false;
            }

            validation.ConfirmedTriggerId = confirmation.TriggerId;
            validation.ConfirmedSceneryId = confirmation.SceneryId;
            validation.ConfirmedFigureId = confirmation.FigureId;
            validation.ConfirmedFrameId = confirmation.FrameId;
            validation.ConfirmedStageId = confirmation.StageId;
            validation.ConfirmedLocationType = confirmation.LocationType.HasValue ? (byte)confirmation.LocationType.Value : null;
            validation.ConfirmedConfirmationType = confirmation.ConfirmationType.HasValue ? (byte)confirmation.ConfirmationType.Value : null;
            validation.ConfirmedIsTrendAligned = confirmation.IsTrendAligned;
            validation.ConfirmedIsPivotZone = confirmation.IsPivotZone;
            validation.ConfirmedAt = confirmation.ConfirmedAt ?? DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(ConfirmAsync), ex);
            throw;
        }
    }

    public async Task<bool> LinkOrderAsync(int validationId, int orderId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _context.AiTradeValidations
                .Where(item => item.Id == validationId && item.UserId == userId && item.OrderId == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.OrderId, orderId), cancellationToken);

            return updated > 0;
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

        if (!HasCompleteVisionExtraction(validation))
        {
            throw new ArgumentException("A complete AI vision extraction is required to save a completed AI trade validation.", nameof(validation));
        }
    }

    private static bool HasCompleteVisionExtraction(AiTradeValidation validation)
    {
        return validation.DetectedTriggerId.HasValue &&
               validation.DetectedSceneryId.HasValue &&
               validation.DetectedFigureId.HasValue &&
               validation.DetectedFrameId.HasValue &&
               validation.DetectedStageId.HasValue &&
               validation.DetectedLocationType.HasValue &&
               validation.DetectedConfirmationType.HasValue &&
               validation.DetectedIsTrendAligned.HasValue &&
               validation.DetectedIsPivotZone.HasValue &&
               validation.VisualConfidence.HasValue;
    }
}
