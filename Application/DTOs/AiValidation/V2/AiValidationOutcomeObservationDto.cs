namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves observed SL/TP flags and observation time without inferring execution sequence.</summary>
/// <param name="ObservedAtUtc">When this outcome observation was recorded, if known.</param>
/// <param name="ReachedSl">The explicitly stored stop-loss outcome flag.</param>
/// <param name="ReachedTp1">The explicitly stored TP1 outcome flag.</param>
/// <param name="ReachedTp2">The explicitly stored TP2 outcome flag.</param>
/// <param name="ReachedTp3">The explicitly stored TP3 outcome flag.</param>
/// <param name="OutcomeCode">An explicitly recorded operational outcome classification.</param>
public sealed record AiValidationOutcomeObservationDto(
    DateTimeOffset? ObservedAtUtc,
    bool? ReachedSl,
    bool? ReachedTp1,
    bool? ReachedTp2,
    bool? ReachedTp3,
    string? OutcomeCode
);
