namespace Application.DTOs.AiValidation.V2;

/// <summary>Describes one source/version-compatible metric group and its field/graphical measurements.</summary>
/// <param name="Versions">The population, provider and exact versions included by this group.</param>
/// <param name="SampleSize">The distinct validation/order sources in the selected population.</param>
/// <param name="FieldMetrics">Denominator-aware field rates pooled from the group's snapshots.</param>
/// <param name="ScenarioGradeConcordance">Manual normalized Scenery versus calculated contextual Grade.</param>
/// <param name="VerdictDistribution">The identified normative graphical verdicts; legacy unassessed rows are excluded.</param>
/// <param name="NotApplicableFieldCount">Justifiably inapplicable schema fields across this group.</param>
/// <param name="NoReviewCount">Applicable fields without human review across this group.</param>
public sealed record AiValidationMetricGroupV2Dto(
    AiValidationMetricVersionGroupV2Dto Versions,
    int SampleSize,
    AiValidationFieldMetricMeasurementV2Dto FieldMetrics,
    RateMetricDto ScenarioGradeConcordance,
    IReadOnlyList<AiValidationMetricVerdictDistributionV2Dto> VerdictDistribution,
    int NotApplicableFieldCount,
    int NoReviewCount
);
