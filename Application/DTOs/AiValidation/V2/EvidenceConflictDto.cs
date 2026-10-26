using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Identifies contradictory observations without replacing the originals with a human confirmation.
/// References identify observations in the associated evidence document, for example by their field paths.
/// Validate this contract with <see cref="Validator"/> at input boundaries.
/// </summary>
/// <param name="FieldCode">The affected business field or evidence path.</param>
/// <param name="ObservationReferences">References to the distinct, contradictory observations.</param>
/// <param name="IsResolved">Whether a reasoned manual resolution has been recorded.</param>
/// <param name="ResolutionObservationReference">A separate human-confirmation observation carrying its source and dates.</param>
/// <param name="ResolutionReason">The rationale for the manual resolution, preserving both original arguments.</param>
public sealed record EvidenceConflictDto(
    string FieldCode,
    IReadOnlyList<string> ObservationReferences,
    bool IsResolved = false,
    string? ResolutionObservationReference = null,
    string? ResolutionReason = null
) : IValidatableObject
{
    /// <summary>Checks that the conflict retains its observations and a resolved conflict has a manual rationale.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the conflict.</param>
    /// <returns>Contract violations; an empty sequence means the conflict is valid.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FieldCode))
        {
            yield return new ValidationResult("A conflict requires an affected field.", [nameof(FieldCode)]);
        }

        if (ObservationReferences is null ||
            ObservationReferences.Count < 2 ||
            ObservationReferences.Any(string.IsNullOrWhiteSpace) ||
            ObservationReferences.Distinct(StringComparer.Ordinal).Count() != ObservationReferences.Count)
        {
            yield return new ValidationResult(
                "A conflict requires at least two distinct observation references.",
                [nameof(ObservationReferences)]);
        }

        if (IsResolved)
        {
            if (string.IsNullOrWhiteSpace(ResolutionObservationReference) ||
                ObservationReferences?.Contains(ResolutionObservationReference, StringComparer.Ordinal) == true)
            {
                yield return new ValidationResult(
                    "A resolved conflict requires a separate human-confirmation observation reference.",
                    [nameof(ResolutionObservationReference)]);
            }

            if (string.IsNullOrWhiteSpace(ResolutionReason))
            {
                yield return new ValidationResult(
                    "A resolved conflict requires a manual rationale.",
                    [nameof(ResolutionReason)]);
            }
        }
        else if (ResolutionObservationReference is not null || ResolutionReason is not null)
        {
            yield return new ValidationResult(
                "An unresolved conflict must not contain a resolution.",
                [nameof(IsResolved), nameof(ResolutionObservationReference), nameof(ResolutionReason)]);
        }
    }
}
