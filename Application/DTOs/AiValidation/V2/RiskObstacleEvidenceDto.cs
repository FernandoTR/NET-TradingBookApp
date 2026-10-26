using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Distinguishes an identified first obstacle, confirmed absence and an unknown map assessment.</summary>
/// <param name="Availability">Availability of the first relevant obstacle in the proposed trade direction.</param>
/// <param name="Reference">The identified map element with its boundaries, timeframe and business reference, when present.</param>
/// <param name="Source">The source of the obstacle assessment, including confirmed absence.</param>
/// <param name="OccurredAtUtc">The business moment of the assessment, when known.</param>
/// <param name="AvailableAtUtc">When the assessment could be known, without backdating a later review.</param>
/// <param name="ReasonCode">An identifiable reason when no obstacle reference is available.</param>
public sealed record RiskObstacleEvidenceDto(
    EvidenceAvailability Availability,
    StructuralReferenceDto? Reference,
    ValidationSource? Source,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? AvailableAtUtc,
    string? ReasonCode
) : IValidatableObject
{
    /// <summary>Checks obstacle availability without selecting or evaluating a map obstacle.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the observation.</param>
    /// <returns>Violations of the obstacle availability contract.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Availability))
        {
            yield return new ValidationResult("Obstacle availability must be a defined state.", [nameof(Availability)]);
            yield break;
        }

        if (Availability == EvidenceAvailability.Present)
        {
            if (Reference is null)
            {
                yield return new ValidationResult("A present obstacle requires a map reference.", [nameof(Reference)]);
            }

            yield break;
        }

        if (Reference is not null)
        {
            yield return new ValidationResult("An unavailable obstacle must not contain a map reference.", [nameof(Reference)]);
        }

        if (string.IsNullOrWhiteSpace(ReasonCode))
        {
            yield return new ValidationResult("An unavailable obstacle requires an identifiable reason.", [nameof(ReasonCode)]);
        }
    }
}
