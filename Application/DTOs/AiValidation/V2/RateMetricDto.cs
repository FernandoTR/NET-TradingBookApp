namespace Application.DTOs.AiValidation.V2;

/// <summary>Reports a rate with its comparable numerator and denominator; a zero denominator is unknown.</summary>
/// <param name="Numerator">The number of observations satisfying the metric.</param>
/// <param name="Denominator">The observations eligible for comparison.</param>
/// <param name="Percentage">The display percentage, or null when no observations are comparable.</param>
public sealed record RateMetricDto(int Numerator, int Denominator, decimal? Percentage);
