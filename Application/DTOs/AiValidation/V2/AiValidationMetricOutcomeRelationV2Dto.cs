using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Describes distinct observed orders by their linked score snapshot and observed operational outcome flags.</summary>
/// <param name="PopulationKind">The population of the selected metric snapshot.</param>
/// <param name="ScoreVersion">The exact score version referenced by the linked immutable score snapshot.</param>
/// <param name="ContextRank">The ordinal rank, when complete and available.</param>
/// <param name="Grade">The calculated Grade, when complete and available.</param>
/// <param name="ScoreValue">The unrounded ordinal score, not a probability.</param>
/// <param name="OrderCount">Distinct orders with at least one recorded operational outcome observation.</param>
/// <param name="OutcomeCodes">The explicitly recorded codes; no SL/TP ordering is inferred.</param>
/// <param name="ObservedAtUtcMin">The earliest known observation time in this group.</param>
/// <param name="ObservedAtUtcMax">The latest known observation time in this group.</param>
/// <param name="ReachedSl">Recorded SL observations over orders with a known SL flag.</param>
/// <param name="ReachedTp1">Recorded TP1 observations over orders with a known TP1 flag.</param>
/// <param name="ReachedTp2">Recorded TP2 observations over orders with a known TP2 flag.</param>
/// <param name="ReachedTp3">Recorded TP3 observations over orders with a known TP3 flag.</param>
public sealed record AiValidationMetricOutcomeRelationV2Dto(
    MetricPopulationKind PopulationKind,
    string? ScoreVersion,
    int? ContextRank,
    GradeType? Grade,
    decimal? ScoreValue,
    int OrderCount,
    IReadOnlyList<string> OutcomeCodes,
    DateTimeOffset? ObservedAtUtcMin,
    DateTimeOffset? ObservedAtUtcMax,
    RateMetricDto ReachedSl,
    RateMetricDto ReachedTp1,
    RateMetricDto ReachedTp2,
    RateMetricDto ReachedTp3
);
