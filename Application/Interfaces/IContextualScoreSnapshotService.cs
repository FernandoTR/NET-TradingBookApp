using Application.DTOs.AiValidation.V2;
using Domain.Entities;

namespace Application.Interfaces;

/// <summary>Explicitly recalculates historical orders and reviewed evaluations into append-only contextual snapshots.</summary>
public interface IContextualScoreSnapshotService
{
    /// <summary>Recalculates distinct historical orders for one user, preserving partial results and per-source failures.</summary>
    Task<ContextualScoreRecalculationBatchDto> RecalculateHistoricalOrdersAsync(
        string userId, IReadOnlyCollection<int> orderIds, CancellationToken cancellationToken);

    /// <summary>Recalculates distinct reviewed validations for one user without inventing missing source revisions.</summary>
    Task<ContextualScoreRecalculationBatchDto> RecalculateReviewedEvaluationsAsync(
        string userId, IReadOnlyCollection<ReviewedEvaluationSnapshotRequestDto> evaluations, CancellationToken cancellationToken);

    /// <summary>Persists an immutable metric review after verifying ownership of its population and linked score.</summary>
    Task<AiTradeValidationMetricSnapshot> SaveMetricSnapshotAsync(
        AiTradeValidationMetricSnapshot snapshot, CancellationToken cancellationToken);
}
