using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Carries one observation without conflating unknown information, confirmed absence and known values.
/// Validate this contract with <see cref="Validator"/> at input boundaries.
/// </summary>
/// <typeparam name="T">The numeric, boolean, temporal or enum value being observed.</typeparam>
/// <param name="Availability">The availability of this observation.</param>
/// <param name="Value">A known value only when availability is Present; false and zero remain known values.</param>
/// <param name="Source">The origin of this particular observation, when known.</param>
/// <param name="OccurredAtUtc">When the observed fact occurred, when known.</param>
/// <param name="AvailableAtUtc">When this observation could be known, without backdating later reviews.</param>
/// <param name="VisualConfidence">Extraction confidence, when known; not a probability of trade success.</param>
/// <param name="ReasonCode">An identifiable reason, required when no value is available.</param>
public sealed record EvidenceValueDto<T>(
    EvidenceAvailability Availability,
    T? Value,
    ValidationSource? Source,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? AvailableAtUtc,
    decimal? VisualConfidence,
    string? ReasonCode
) : IValidatableObject where T : struct
{
    /// <summary>Checks the availability/value relationship and the reason for an unavailable value.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the observation.</param>
    /// <returns>Contract violations; an empty sequence means the observation is valid.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Availability))
        {
            yield return new ValidationResult(
                "Evidence availability must be a defined state.",
                [nameof(Availability)]);
            yield break;
        }

        if (Availability == EvidenceAvailability.Present)
        {
            if (!Value.HasValue)
            {
                yield return new ValidationResult(
                    "Present evidence requires a value.",
                    [nameof(Value)]);
            }

            yield break;
        }

        if (Value.HasValue)
        {
            yield return new ValidationResult(
                "Unknown, absent and not applicable evidence must not contain a value.",
                [nameof(Value)]);
        }

        if (string.IsNullOrWhiteSpace(ReasonCode))
        {
            yield return new ValidationResult(
                "Evidence without a value requires an identifiable reason.",
                [nameof(ReasonCode)]);
        }
    }
}
