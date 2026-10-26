using System.Text.Json;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>Stores immutable metric snapshots and filters every read by user and population.</summary>
/// <param name="context">The database-first application context with protected snapshot mappings.</param>
public sealed class AiTradeValidationMetricSnapshotRepository(ApplicationDbContext context) : IAiTradeValidationMetricSnapshotRepository
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<AiTradeValidationMetricSnapshot> AddAsync(AiTradeValidationMetricSnapshot snapshot, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        ValidateSnapshot(snapshot);

        if (!await _context.Users.AsNoTracking().AnyAsync(user => user.Id == snapshot.UserId, cancellationToken))
            throw new InvalidOperationException("METRIC_SNAPSHOT_OWNER_NOT_FOUND");
        if (snapshot.OrderId.HasValue &&
            !await _context.Orders.AsNoTracking().AnyAsync(order => order.Id == snapshot.OrderId && order.AuthorId == snapshot.UserId, cancellationToken))
            throw new InvalidOperationException("METRIC_SNAPSHOT_ORDER_NOT_OWNED");

        AiTradeValidation? validation = null;
        if (snapshot.ValidationId.HasValue)
        {
            validation = await _context.AiTradeValidations.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == snapshot.ValidationId && item.UserId == snapshot.UserId, cancellationToken);
            if (validation is null) throw new InvalidOperationException("METRIC_SNAPSHOT_VALIDATION_NOT_OWNED");
        }

        if (snapshot.OrderId.HasValue && validation?.OrderId is int linkedOrderId && linkedOrderId != snapshot.OrderId.Value)
            throw new InvalidOperationException("METRIC_SNAPSHOT_SOURCE_ORDER_MISMATCH");
        if (snapshot.ContextualScoreSnapshotId.HasValue &&
            !await _context.ContextualScoreSnapshots.AsNoTracking().AnyAsync(
                score => score.Id == snapshot.ContextualScoreSnapshotId && score.UserId == snapshot.UserId, cancellationToken))
            throw new InvalidOperationException("METRIC_SNAPSHOT_SCORE_NOT_OWNED");

        await _context.AiTradeValidationMetricSnapshots.AddAsync(snapshot, cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return snapshot;
        }
        catch
        {
            _context.Entry(snapshot).State = EntityState.Detached;
            throw;
        }
    }

    /// <inheritdoc />
    public Task<AiTradeValidationMetricSnapshot?> GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        if (snapshotId <= 0) throw new ArgumentOutOfRangeException(nameof(snapshotId));
        return _context.AiTradeValidationMetricSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(snapshot => snapshot.Id == snapshotId && snapshot.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(
        string userId, MetricPopulationKind populationKind, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        if (!Enum.IsDefined(populationKind)) throw new ArgumentOutOfRangeException(nameof(populationKind));
        return await _context.AiTradeValidationMetricSnapshots.AsNoTracking()
            .Where(snapshot => snapshot.UserId == userId && snapshot.PopulationKind == populationKind)
            .OrderBy(snapshot => snapshot.CapturedAtUtc).ThenBy(snapshot => snapshot.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(
        string userId, AiValidationMetricFilterV2Dto filter, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(filter);
        if (filter.PopulationKind.HasValue && !Enum.IsDefined(filter.PopulationKind.Value))
            throw new ArgumentOutOfRangeException(nameof(filter.PopulationKind));
        if (filter.FromUtc.HasValue && filter.ToUtc.HasValue && filter.FromUtc.Value > filter.ToUtc.Value)
            throw new ArgumentException("METRIC_FILTER_DATE_RANGE_INVALID", nameof(filter));
        if (filter.InstrumentId is <= 0 || filter.StageId is <= 0 || filter.TriggerId is <= 0)
            throw new ArgumentOutOfRangeException(nameof(filter), "Metric dimension IDs must be positive when supplied.");

        var query = _context.AiTradeValidationMetricSnapshots.AsNoTracking().Where(snapshot => snapshot.UserId == userId);
        if (filter.PopulationKind.HasValue) query = query.Where(snapshot => snapshot.PopulationKind == filter.PopulationKind.Value);
        if (!string.IsNullOrWhiteSpace(filter.ProviderName)) query = query.Where(snapshot => snapshot.ProviderName == filter.ProviderName.Trim());
        if (!string.IsNullOrWhiteSpace(filter.ModelName)) query = query.Where(snapshot => snapshot.ModelName == filter.ModelName.Trim());
        if (filter.FromUtc.HasValue) query = query.Where(snapshot => snapshot.CapturedAtUtc >= filter.FromUtc.Value);
        if (filter.ToUtc.HasValue) query = query.Where(snapshot => snapshot.CapturedAtUtc <= filter.ToUtc.Value);
        if (filter.InstrumentId.HasValue) query = query.Where(snapshot => snapshot.InstrumentId == filter.InstrumentId.Value);
        if (filter.StageId.HasValue) query = query.Where(snapshot => snapshot.StageId == filter.StageId.Value);
        if (filter.TriggerId.HasValue) query = query.Where(snapshot => snapshot.TriggerId == filter.TriggerId.Value);
        if (!string.IsNullOrWhiteSpace(filter.StrategyVersion)) query = query.Where(snapshot => snapshot.StrategyVersion == filter.StrategyVersion);
        if (!string.IsNullOrWhiteSpace(filter.EvidenceSchemaVersion)) query = query.Where(snapshot => snapshot.EvidenceSchemaVersion == filter.EvidenceSchemaVersion);
        if (!string.IsNullOrWhiteSpace(filter.GraphicalRulesVersion)) query = query.Where(snapshot => snapshot.GraphicalRulesVersion == filter.GraphicalRulesVersion);
        if (!string.IsNullOrWhiteSpace(filter.ClassificationRulesVersion)) query = query.Where(snapshot => snapshot.ClassificationRulesVersion == filter.ClassificationRulesVersion);
        if (!string.IsNullOrWhiteSpace(filter.ScoreVersion)) query = query.Where(snapshot => snapshot.ScoreVersion == filter.ScoreVersion);
        if (!string.IsNullOrWhiteSpace(filter.MetricsVersion)) query = query.Where(snapshot => snapshot.MetricsVersion == filter.MetricsVersion);
        if (!string.IsNullOrWhiteSpace(filter.PromptVersion)) query = query.Where(snapshot => snapshot.PromptVersion == filter.PromptVersion);
        if (!string.IsNullOrWhiteSpace(filter.ExtractionSchemaVersion)) query = query.Where(snapshot => snapshot.ExtractionSchemaVersion == filter.ExtractionSchemaVersion);

        return await query.OrderBy(snapshot => snapshot.CapturedAtUtc).ThenBy(snapshot => snapshot.Id).ToListAsync(cancellationToken);
    }

    private static void ValidateSnapshot(AiTradeValidationMetricSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.UserId)) throw new ArgumentException("METRIC_SNAPSHOT_USER_REQUIRED", nameof(snapshot));
        if (!Enum.IsDefined(snapshot.PopulationKind)) throw new ArgumentOutOfRangeException(nameof(snapshot.PopulationKind));
        if (snapshot.PopulationKind == MetricPopulationKind.HistoricalOrder && snapshot.OrderId is not > 0 ||
            snapshot.PopulationKind == MetricPopulationKind.ReviewedEvaluation && snapshot.ValidationId is not > 0)
            throw new ArgumentException("METRIC_SNAPSHOT_SOURCE_ID_REQUIRED", nameof(snapshot));
        if (snapshot.OrderId is <= 0 || snapshot.ValidationId is <= 0 || snapshot.ContextualScoreSnapshotId is <= 0 ||
            snapshot.EvaluationRevision is <= 0 || snapshot.CapturedAtUtc == default)
            throw new ArgumentException("METRIC_SNAPSHOT_SOURCE_ID_INVALID", nameof(snapshot));
        ValidateJson(snapshot.FieldsJson, nameof(snapshot.FieldsJson));
        ValidateJson(snapshot.GraphicResultsJson, nameof(snapshot.GraphicResultsJson), optional: true);
        ValidateJson(snapshot.OutcomeObservationsJson, nameof(snapshot.OutcomeObservationsJson), optional: true);
    }

    private static void ValidateJson(string? json, string name, bool optional = false)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            if (optional) return;
            throw new ArgumentException("METRIC_SNAPSHOT_JSON_REQUIRED", name);
        }

        try { using var _ = JsonDocument.Parse(json); }
        catch (JsonException exception) { throw new ArgumentException("METRIC_SNAPSHOT_JSON_INVALID", name, exception); }
    }

    private static void ValidateUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("METRIC_SNAPSHOT_USER_REQUIRED", nameof(userId));
    }
}
