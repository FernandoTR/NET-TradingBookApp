namespace Application.DTOs.AiValidation.V2;

/// <summary>Records supplied convergence evidence without inferring it from a single selected figure.</summary>
/// <param name="ReferenceCode">The business reference identifying the proposed convergence.</param>
/// <param name="StructuralReferenceCodes">The distinct map structures involved, carrying their own directions and frames.</param>
/// <param name="IsPresent">Whether convergence is observed, explicitly absent or still unknown.</param>
/// <param name="EvidenceReferences">The observations supporting the convergence assessment.</param>
/// <param name="Rationale">The supplied argument for the assessment.</param>
public sealed record ContextualConvergenceEvidenceDto(
    string ReferenceCode,
    IReadOnlyList<string> StructuralReferenceCodes,
    EvidenceValueDto<bool> IsPresent,
    IReadOnlyList<string> EvidenceReferences,
    string? Rationale
);
