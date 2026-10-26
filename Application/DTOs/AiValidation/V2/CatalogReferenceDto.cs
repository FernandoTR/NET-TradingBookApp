using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Preserves a recognized catalog concept independently of its normalized database identifier.
/// Validate this contract with <see cref="Validator"/> at input boundaries.
/// </summary>
/// <param name="Availability">Whether the business concept is known, absent, unknown or inapplicable.</param>
/// <param name="ConceptCode">A semantic business code, when identified; not an assumed database ID.</param>
/// <param name="ObservedLabel">The supplied business label, preserved even before catalog normalization.</param>
/// <param name="NormalizedId">The catalog ID with independent availability and normalization provenance.</param>
/// <param name="Source">The source identifying the concept, independently of the ID's source.</param>
/// <param name="OccurredAtUtc">When the referenced business fact occurred, when known.</param>
/// <param name="AvailableAtUtc">When the concept identification could be known, when known.</param>
/// <param name="ReasonCode">An identifiable reason when the concept is not present.</param>
public sealed record CatalogReferenceDto(
    EvidenceAvailability Availability,
    string? ConceptCode,
    string? ObservedLabel,
    EvidenceValueDto<int> NormalizedId,
    ValidationSource? Source,
    DateTimeOffset? OccurredAtUtc,
    DateTimeOffset? AvailableAtUtc,
    string? ReasonCode
) : IValidatableObject
{
    /// <summary>Checks concept availability while allowing a known concept with an unresolved catalog ID.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the reference.</param>
    /// <returns>Contract violations, including violations in the normalized-ID observation.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NormalizedId is null)
        {
            yield return new ValidationResult("Catalog ID availability must be explicit.", [nameof(NormalizedId)]);
        }
        else
        {
            foreach (var error in NormalizedId.Validate(new ValidationContext(NormalizedId)))
            {
                yield return new ValidationResult(
                    error.ErrorMessage,
                    error.MemberNames.Select(member => $"{nameof(NormalizedId)}.{member}"));
            }
        }

        if (!Enum.IsDefined(Availability))
        {
            yield return new ValidationResult("Concept availability must be a defined state.", [nameof(Availability)]);
            yield break;
        }

        if (Availability == EvidenceAvailability.Present)
        {
            if (string.IsNullOrWhiteSpace(ConceptCode) && string.IsNullOrWhiteSpace(ObservedLabel) &&
                !(NormalizedId?.Availability == EvidenceAvailability.Present && NormalizedId.Value.HasValue))
            {
                yield return new ValidationResult(
                    "A present catalog reference requires a concept, label or known catalog ID.",
                    [nameof(ConceptCode), nameof(ObservedLabel), nameof(NormalizedId)]);
            }

            yield break;
        }

        if (ConceptCode is not null || ObservedLabel is not null || NormalizedId?.Availability == EvidenceAvailability.Present)
        {
            yield return new ValidationResult(
                "A concept that is unknown, absent or not applicable must not claim a label, code or known catalog ID.",
                [nameof(ConceptCode), nameof(ObservedLabel), nameof(NormalizedId)]);
        }

        if (string.IsNullOrWhiteSpace(ReasonCode))
        {
            yield return new ValidationResult("A concept that is not present requires an identifiable reason.", [nameof(ReasonCode)]);
        }
    }
}
