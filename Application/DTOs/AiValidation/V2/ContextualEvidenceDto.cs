namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Separates the primary figure, structural references, convergences and additional factors.
/// Empty lists do not prove absence: completeness must itself be an explicit, sourced observation.
/// </summary>
/// <param name="MainFigure">The primary figure concept and independently normalized catalog ID.</param>
/// <param name="ClassificationStructureReferenceCode">The structure selected as the classification argument, or null when unknown.</param>
/// <param name="StructuralReferences">The identified primary and additional structures, including their directions and timeframes.</param>
/// <param name="StructuralReferencesComplete">Whether the reviewed collection of structural references is complete.</param>
/// <param name="Convergences">The supplied assessments linking map structures.</param>
/// <param name="ConvergencesComplete">Whether the convergence review is complete; a selected figure alone is insufficient.</param>
/// <param name="Factors">The favorable, contrary or neutral factor observations, preserving opposing arguments.</param>
/// <param name="FactorsComplete">Whether all applicable additional factors have been reviewed.</param>
public sealed record ContextualEvidenceDto(
    CatalogReferenceDto MainFigure,
    string? ClassificationStructureReferenceCode,
    IReadOnlyList<StructuralReferenceDto> StructuralReferences,
    EvidenceValueDto<bool> StructuralReferencesComplete,
    IReadOnlyList<ContextualConvergenceEvidenceDto> Convergences,
    EvidenceValueDto<bool> ConvergencesComplete,
    IReadOnlyList<ContextualFactorEvidenceDto> Factors,
    EvidenceValueDto<bool> FactorsComplete
);
