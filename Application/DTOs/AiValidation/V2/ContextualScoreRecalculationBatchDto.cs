using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Reports distinct source outcomes from an explicit, user-scoped recalculation request.</summary>
/// <param name="SourceKind">The population selected for this batch.</param>
/// <param name="RequestedSourceCount">The number of distinct source IDs after duplicate removal.</param>
/// <param name="Results">One result for each distinct source, preserving partial outcomes and failures.</param>
public sealed record ContextualScoreRecalculationBatchDto(
    ContextualScoreSourceKind SourceKind,
    int RequestedSourceCount,
    IReadOnlyList<ContextualScoreRecalculationItemDto> Results
);
