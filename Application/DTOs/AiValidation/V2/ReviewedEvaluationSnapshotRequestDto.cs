namespace Application.DTOs.AiValidation.V2;

/// <summary>Identifies a reviewed validation source without inventing revisions for legacy rows.</summary>
/// <param name="ValidationId">The validation ID, always scoped to the requesting owner.</param>
/// <param name="EvaluationRevision">The known reviewed revision, or null when the source predates revision tracking.</param>
public sealed record ReviewedEvaluationSnapshotRequestDto(int ValidationId, int? EvaluationRevision = null);
