using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>Stores immutable score rows and filters every source lookup by its identity owner.</summary>
/// <param name="context">The database-first application context with protected snapshot mappings.</param>
public sealed class ContextualScoreSnapshotRepository(ApplicationDbContext context) : IContextualScoreSnapshotRepository
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<ContextualScoreSnapshot> AddAsync(ContextualScoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        cancellationToken.ThrowIfCancellationRequested();
        ValidateSnapshot(snapshot);

        if (!await _context.Users.AsNoTracking().AnyAsync(user => user.Id == snapshot.UserId, cancellationToken))
            throw new InvalidOperationException("SNAPSHOT_OWNER_NOT_FOUND");

        if (snapshot.OrderId.HasValue &&
            !await _context.Orders.AsNoTracking().AnyAsync(order => order.Id == snapshot.OrderId && order.AuthorId == snapshot.UserId, cancellationToken))
            throw new InvalidOperationException("SNAPSHOT_ORDER_NOT_OWNED");

        AiTradeValidation? validation = null;
        if (snapshot.ValidationId.HasValue)
        {
            validation = await _context.AiTradeValidations.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == snapshot.ValidationId && item.UserId == snapshot.UserId, cancellationToken);
            if (validation is null) throw new InvalidOperationException("SNAPSHOT_VALIDATION_NOT_OWNED");
        }

        if (snapshot.OrderId.HasValue && validation?.OrderId is int linkedOrderId && linkedOrderId != snapshot.OrderId.Value)
            throw new InvalidOperationException("SNAPSHOT_SOURCE_ORDER_MISMATCH");

        await _context.ContextualScoreSnapshots.AddAsync(snapshot, cancellationToken);
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
    public Task<ContextualScoreSnapshot?> GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        if (snapshotId <= 0) throw new ArgumentOutOfRangeException(nameof(snapshotId));
        return _context.ContextualScoreSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(snapshot => snapshot.Id == snapshotId && snapshot.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContextualScoreSnapshot>> GetByIdsAsync(
        string userId, IReadOnlyCollection<int> snapshotIds, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(snapshotIds);
        if (snapshotIds.Any(id => id <= 0)) throw new ArgumentOutOfRangeException(nameof(snapshotIds));
        var ids = snapshotIds.Distinct().ToArray();
        if (ids.Length == 0) return [];
        return await _context.ContextualScoreSnapshots.AsNoTracking()
            .Where(snapshot => snapshot.UserId == userId && ids.Contains(snapshot.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContextualScoreSnapshot>> GetBySourceAsync(
        string userId, ContextualScoreSourceKind sourceKind, int? orderId, int? validationId, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        if (!Enum.IsDefined(sourceKind)) throw new ArgumentOutOfRangeException(nameof(sourceKind));
        if (orderId is <= 0 || validationId is <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        var hasOrder = orderId is > 0;
        var hasValidation = validationId is > 0;
        if (!hasOrder && !hasValidation) throw new ArgumentException("At least one source ID is required.", nameof(orderId));

        var query = _context.ContextualScoreSnapshots.AsNoTracking()
            .Where(snapshot => snapshot.UserId == userId && snapshot.SourceKind == sourceKind);
        if (hasOrder) query = query.Where(snapshot => snapshot.OrderId == orderId);
        if (hasValidation) query = query.Where(snapshot => snapshot.ValidationId == validationId);
        return await query.OrderBy(snapshot => snapshot.CalculatedAtUtc).ThenBy(snapshot => snapshot.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Infrastructure.Order?> GetOrderForUserAsync(int orderId, string userId, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        return _context.Orders.AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == orderId && order.AuthorId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AiTradeValidation?> GetValidationForUserAsync(int validationId, string userId, CancellationToken cancellationToken)
    {
        ValidateUserId(userId);
        if (validationId <= 0) throw new ArgumentOutOfRangeException(nameof(validationId));
        return _context.AiTradeValidations.AsNoTracking()
            .FirstOrDefaultAsync(validation => validation.Id == validationId && validation.UserId == userId, cancellationToken);
    }

    private static void ValidateSnapshot(ContextualScoreSnapshot snapshot)
    {
        ValidateUserId(snapshot.UserId);
        if (!Enum.IsDefined(snapshot.SourceKind) || !Enum.IsDefined(snapshot.State))
            throw new ArgumentException("SNAPSHOT_ENUM_VALUE_INVALID", nameof(snapshot));
        if (snapshot.SourceKind == ContextualScoreSourceKind.HistoricalOrder && snapshot.OrderId is not > 0 ||
            snapshot.SourceKind == ContextualScoreSourceKind.ReviewedEvaluation && snapshot.ValidationId is not > 0)
            throw new ArgumentException("SNAPSHOT_SOURCE_ID_REQUIRED", nameof(snapshot));
        if (snapshot.OrderId is <= 0 || snapshot.ValidationId is <= 0 || snapshot.EvaluationRevision is <= 0)
            throw new ArgumentException("SNAPSHOT_SOURCE_ID_INVALID", nameof(snapshot));
        if (string.IsNullOrWhiteSpace(snapshot.ScoreVersion) || snapshot.CalculatedAtUtc == default)
            throw new ArgumentException("SNAPSHOT_VERSION_AND_TIMESTAMP_REQUIRED", nameof(snapshot));

        ValidateJson(snapshot.DefinitionJson, nameof(snapshot.DefinitionJson));
        ValidateJson(snapshot.InputSnapshotJson, nameof(snapshot.InputSnapshotJson));
        ValidateJson(snapshot.ComponentsJson, nameof(snapshot.ComponentsJson));

        var complete = snapshot.State == ContextualScoreState.Complete;
        if (complete != (snapshot.ContextRank.HasValue && snapshot.Value.HasValue && snapshot.Grade.HasValue) ||
            snapshot.ContextRank is < 0 or > 6 || snapshot.Grade.HasValue && !Enum.IsDefined(snapshot.Grade.Value))
            throw new ArgumentException("SNAPSHOT_SCORE_STATE_INCONSISTENT", nameof(snapshot));

        ValidateDefinition(snapshot, complete);

    }

    private static void ValidateJson(string? json, string name)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("SNAPSHOT_JSON_REQUIRED", name);
        try { using var _ = JsonDocument.Parse(json); }
        catch (JsonException exception) { throw new ArgumentException("SNAPSHOT_JSON_INVALID", name, exception); }
    }

    private static void ValidateDefinition(ContextualScoreSnapshot snapshot, bool complete)
    {
        JsonDocument definition;
        try { definition = JsonDocument.Parse(snapshot.DefinitionJson); }
        catch (JsonException exception) { throw new ArgumentException("SNAPSHOT_JSON_INVALID", nameof(snapshot.DefinitionJson), exception); }

        using (definition)
        {
            var root = definition.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("scoreVersion", out var version) || version.ValueKind != JsonValueKind.String ||
                version.GetString() != snapshot.ScoreVersion ||
                !root.TryGetProperty("positions", out var positionsElement) || positionsElement.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("SNAPSHOT_SCORE_DEFINITION_INVALID", nameof(snapshot));

            var positions = positionsElement.EnumerateArray().Select(position =>
            {
                if (position.ValueKind != JsonValueKind.Object || !position.TryGetProperty("grade", out var gradeElement) ||
                    !position.TryGetProperty("contextRank", out var rankElement) || !rankElement.TryGetInt32(out var rank))
                    throw new ArgumentException("SNAPSHOT_SCORE_DEFINITION_INVALID", nameof(snapshot));
                var grade = ReadGrade(gradeElement);
                if (!grade.HasValue || rank is < 0 or > 6) throw new ArgumentException("SNAPSHOT_SCORE_DEFINITION_INVALID", nameof(snapshot));
                return (Grade: grade.Value, Rank: rank);
            }).ToArray();

            if (positions.Length != 7 || positions.Select(position => position.Grade).Distinct().Count() != 7 ||
                positions.Select(position => position.Rank).Distinct().Count() != 7)
                throw new ArgumentException("SNAPSHOT_SCORE_DEFINITION_INVALID", nameof(snapshot));

            if (complete)
            {
                var matching = positions.Where(position => position.Grade == snapshot.Grade).ToArray();
                if (matching.Length != 1 || matching[0].Rank != snapshot.ContextRank ||
                    snapshot.Value != 100m * snapshot.ContextRank!.Value / 6m)
                    throw new ArgumentException("SNAPSHOT_GRADE_RANK_VALUE_INCONSISTENT", nameof(snapshot));
            }
        }
    }

    private static GradeType? ReadGrade(JsonElement value)
    {
        var name = value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetInt32(out var numeric) && Enum.IsDefined(typeof(GradeType), numeric) =>
                Enum.GetName(typeof(GradeType), numeric),
            _ => null
        };
        return Enum.TryParse<GradeType>(name, ignoreCase: false, out var grade) && Enum.IsDefined(grade) ? grade : null;
    }

    private static void ValidateUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("SNAPSHOT_USER_REQUIRED", nameof(userId));
    }
}
