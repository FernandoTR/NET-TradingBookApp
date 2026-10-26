using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Returns a versioned ordinal score without conflating a complete C/zero result with missing information.</summary>
/// <param name="ScoreVersion">The applied ordinal-score definition version.</param>
/// <param name="State">Availability of the contextual result.</param>
/// <param name="ContextRank">The internal position from 0 to 6, unknown for partial or non-evaluable results.</param>
/// <param name="Value">The unrounded decimal value of 100 times position divided by 6; not a win probability.</param>
/// <param name="Grade">The Grade obtained from the explicit position table before presentation rounding.</param>
/// <param name="ExpectedComponents">The number of applicable data and checks required by the rule, not a weighted total.</param>
/// <param name="EvaluatedComponents">The applicable components with determined information, including confirmed absence.</param>
/// <param name="Components">The checks, provenance, reasons and evidence references used in this evaluation.</param>
public sealed record ContextualScoreResultDto(
    string ScoreVersion,
    ContextualScoreState State,
    int? ContextRank,
    decimal? Value,
    GradeType? Grade,
    int ExpectedComponents,
    int EvaluatedComponents,
    IReadOnlyList<ContextualScoreComponentDto> Components
);
