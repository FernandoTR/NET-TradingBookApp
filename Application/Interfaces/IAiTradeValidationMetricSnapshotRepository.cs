using Domain.Entities;
using Domain.Enums;
using Application.DTOs.AiValidation.V2;

namespace Application.Interfaces;

/// <summary>Persists and queries immutable, population-identified metric snapshots by owner.</summary>
public interface IAiTradeValidationMetricSnapshotRepository
{
    /// <summary>Saves a new metric snapshot after verifying the owner and all related snapshot sources.</summary>
    Task<AiTradeValidationMetricSnapshot> AddAsync(AiTradeValidationMetricSnapshot snapshot, CancellationToken cancellationToken);

    /// <summary>Gets a metric snapshot only if it belongs to the requesting user.</summary>
    Task<AiTradeValidationMetricSnapshot?> GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken);

    /// <summary>Gets snapshots for one owner and source population, sorted by capture time.</summary>
    Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(
        string userId, MetricPopulationKind populationKind, CancellationToken cancellationToken);

    /// <summary>Queries the authenticated user's metric rows using the selected population and recorded dimensions.</summary>
    Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(
        string userId, AiValidationMetricFilterV2Dto filter, CancellationToken cancellationToken);
}
