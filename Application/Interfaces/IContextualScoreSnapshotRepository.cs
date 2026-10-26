using Domain.Entities;
using Domain.Enums;
using Infrastructure;

namespace Application.Interfaces;

/// <summary>Persists immutable contextual snapshots and returns source rows only within their owner's scope.</summary>
public interface IContextualScoreSnapshotRepository
{
    /// <summary>Saves a new snapshot after verifying the owner and referenced source rows.</summary>
    Task<ContextualScoreSnapshot> AddAsync(ContextualScoreSnapshot snapshot, CancellationToken cancellationToken);

    /// <summary>Gets a snapshot only if it belongs to the requesting user.</summary>
    Task<ContextualScoreSnapshot?> GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken);

    /// <summary>Gets a set of score snapshots for one owner in one query, excluding every other user's values.</summary>
    Task<IReadOnlyList<ContextualScoreSnapshot>> GetByIdsAsync(
        string userId, IReadOnlyCollection<int> snapshotIds, CancellationToken cancellationToken);

    /// <summary>Gets snapshots for one owner and source population, sorted by calculation time.</summary>
    Task<IReadOnlyList<ContextualScoreSnapshot>> GetBySourceAsync(
        string userId, ContextualScoreSourceKind sourceKind, int? orderId, int? validationId, CancellationToken cancellationToken);

    /// <summary>Loads a historical order only when its AuthorId matches the requesting owner.</summary>
    Task<Order?> GetOrderForUserAsync(int orderId, string userId, CancellationToken cancellationToken);

    /// <summary>Loads a validation only when its UserId matches the requesting owner.</summary>
    Task<AiTradeValidation?> GetValidationForUserAsync(int validationId, string userId, CancellationToken cancellationToken);
}
