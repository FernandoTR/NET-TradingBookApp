using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Reports an identified graphical verdict over comparable evaluations with a non-null verdict.</summary>
/// <param name="Verdict">The normative pending, valid or invalid graphical verdict.</param>
/// <param name="Count">The number of distinct included evaluations with this verdict.</param>
/// <param name="Denominator">All distinct included evaluations with an identified verdict.</param>
/// <param name="Percentage">The display percentage, or null when no verdicts were identified.</param>
public sealed record AiValidationMetricVerdictDistributionV2Dto(
    SetupVerdict Verdict,
    int Count,
    int Denominator,
    decimal? Percentage
);
