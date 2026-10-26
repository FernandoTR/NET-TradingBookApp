namespace Application.DTOs.AiValidation.V2;

/// <summary>Summarizes one validation's field measurements with explicit, independently eligible denominators.</summary>
/// <param name="MetricsVersion">The definition of the rates, or null when the source snapshot version is unknown.</param>
/// <param name="Coverage">Known AI values over expected applicable fields.</param>
/// <param name="Concordance">Matching values over comparable, reviewed field pairs.</param>
/// <param name="HumanCorrections">Corrected/discarded AI assertions over known AI assertions reviewed by a human.</param>
/// <param name="ManualCompletions">Human-known values over AI-unknown fields actually reviewed.</param>
/// <param name="Discards">Reviewed fields explicitly left undetermined over all reviewed applicable fields.</param>
/// <param name="Fields">The field-level evidence and reason codes used by these measurements.</param>
/// <param name="NotApplicableFieldCount">Expected schema fields excluded with an explicit inapplicability reason.</param>
/// <param name="NoReviewCount">Applicable fields for which no human review was recorded.</param>
public sealed record AiValidationFieldMetricMeasurementV2Dto(
    string? MetricsVersion,
    RateMetricDto Coverage,
    RateMetricDto Concordance,
    RateMetricDto HumanCorrections,
    RateMetricDto ManualCompletions,
    RateMetricDto Discards,
    IReadOnlyList<AiValidationFieldMetricDto> Fields,
    int NotApplicableFieldCount,
    int NoReviewCount
);
