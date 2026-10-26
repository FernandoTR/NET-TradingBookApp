using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Aggregates reviewed validation metrics and descriptive order outcomes by comparable populations and versions.</summary>
/// <param name="SampleSize">The selected source rows after population-specific revision/order deduplication.</param>
/// <param name="DistinctValidationCount">Distinct validation sources included in the reviewed population.</param>
/// <param name="DistinctOrderCount">Distinct order sources included for descriptive outcome analysis.</param>
/// <param name="Groups">Population/version groups; percentages are not averaged across incompatible denominators.</param>
/// <param name="OutcomeRelationships">Distinct observed orders grouped by their referenced score version, rank and Grade.</param>
/// <param name="IncludedPopulations">The populations represented, including the empty set when no sources match.</param>
public sealed record AiValidationMetricSummaryV2Dto(
    int SampleSize,
    int DistinctValidationCount,
    int DistinctOrderCount,
    IReadOnlyList<AiValidationMetricGroupV2Dto> Groups,
    IReadOnlyList<AiValidationMetricOutcomeRelationV2Dto> OutcomeRelationships,
    IReadOnlyList<MetricPopulationKind> IncludedPopulations
);
