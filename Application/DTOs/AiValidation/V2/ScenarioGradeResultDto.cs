using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves the contextual classification and its arguments for the same evaluation that will produce the ordinal score.</summary>
/// <param name="ClassificationRulesVersion">The applied classification definition.</param>
/// <param name="State">Whether the contextual classification is complete, partial or not evaluable.</param>
/// <param name="BaseContext">The established base context, including the deterministic pivot-line base.</param>
/// <param name="Grade">The determinable Grade, otherwise null.</param>
/// <param name="RuleCode">The matching contextual rule, when determinable.</param>
/// <param name="Justification">The rule's contextual explanation, when available.</param>
/// <param name="Components">The data and checks required by the applicable rule.</param>
/// <param name="StructuralReferences">The supplied structural arguments, with directions and timeframes.</param>
/// <param name="Convergences">The supplied convergence arguments.</param>
/// <param name="Factors">All supplied factor observations, retaining favorable and contrary arguments.</param>
/// <param name="PredominanceResolution">The supplied manual decision, when any, without replacing the factor observations.</param>
/// <param name="ReasonCodes">Classification, missing-information and conflict reasons.</param>
/// <param name="EvidenceReferences">References used in the classification checks.</param>
public sealed record ScenarioGradeResultDto(
    string ClassificationRulesVersion,
    ContextualScoreState State,
    EvidenceValueDto<ContextualBaseContext> BaseContext,
    GradeType? Grade,
    string? RuleCode,
    string? Justification,
    IReadOnlyList<ContextualScoreComponentDto> Components,
    IReadOnlyList<StructuralReferenceDto> StructuralReferences,
    IReadOnlyList<ContextualConvergenceEvidenceDto> Convergences,
    IReadOnlyList<ContextualFactorEvidenceDto> Factors,
    ContextualPredominanceResolutionDto? PredominanceResolution,
    IReadOnlyList<string> ReasonCodes,
    IReadOnlyList<string> EvidenceReferences
);
