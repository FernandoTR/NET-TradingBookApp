using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Reports one source-specific recalculation outcome without hiding partial or missing evidence.</summary>
/// <param name="SourceKind">The identified population for the attempted calculation.</param>
/// <param name="OrderId">The historical order source, when applicable.</param>
/// <param name="ValidationId">The reviewed validation source, when applicable.</param>
/// <param name="EvaluationRevision">The recorded source revision, if it exists.</param>
/// <param name="Succeeded">Whether an immutable snapshot was saved.</param>
/// <param name="SnapshotId">The saved snapshot ID, when successful.</param>
/// <param name="State">The saved score availability, including partial or not evaluable outcomes.</param>
/// <param name="FailureCode">A stable failure code if the source was unavailable or could not be saved.</param>
public sealed record ContextualScoreRecalculationItemDto(
    ContextualScoreSourceKind SourceKind,
    int? OrderId,
    int? ValidationId,
    int? EvaluationRevision,
    bool Succeeded,
    int? SnapshotId,
    ContextualScoreState? State,
    string? FailureCode
);
