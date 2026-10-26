using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Provides a canonical scalar for heterogeneous validation fields while preserving unknown, absent and N/A states.
/// The normalized string is a business scalar (for example "false", "0" or an enum code), never file metadata.
/// </summary>
/// <param name="Availability">Availability of this field observation.</param>
/// <param name="NormalizedValue">Canonical comparable value only when availability is Present.</param>
/// <param name="Source">The observation's origin when known.</param>
/// <param name="OccurredAtUtc">When the underlying fact occurred, when known.</param>
/// <param name="AvailableAtUtc">When this observation could be known, without backdating a later review.</param>
/// <param name="ReasonCode">An identifiable reason for unknown, absent or inapplicable evidence.</param>
public sealed record AiValidationFieldObservationDto(
    EvidenceAvailability Availability,
    string? NormalizedValue,
    ValidationSource? Source,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? AvailableAtUtc,
    string? ReasonCode
) : IValidatableObject
{
    /// <summary>Checks that a present scalar and unavailable reason follow the evidence availability contract.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Availability))
        {
            yield return new ValidationResult("Field availability must be a defined state.", [nameof(Availability)]);
            yield break;
        }

        if (Availability == EvidenceAvailability.Present)
        {
            if (NormalizedValue is null)
                yield return new ValidationResult("Present field observations require a canonical value.", [nameof(NormalizedValue)]);
            yield break;
        }

        if (NormalizedValue is not null)
            yield return new ValidationResult("Unavailable field observations must not contain a value.", [nameof(NormalizedValue)]);

        if (string.IsNullOrWhiteSpace(ReasonCode))
            yield return new ValidationResult("Unavailable field observations require a reason.", [nameof(ReasonCode)]);
    }
}
