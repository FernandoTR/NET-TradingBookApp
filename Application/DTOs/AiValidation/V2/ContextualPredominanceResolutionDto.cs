using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Records a human assessment of contextual predominance while retaining both opposing arguments, never a manual Grade.</summary>
/// <param name="Predominance">The favorable, contrary or neutral predominance selected by the reviewer.</param>
/// <param name="ReviewerUserId">The user responsible for this decision.</param>
/// <param name="ResolvedAtUtc">When the decision was made; it does not backdate information availability.</param>
/// <param name="Rationale">The reasoned foundation of the decision.</param>
/// <param name="FavorableArguments">The favorable observations considered, retained as business-data snapshots.</param>
/// <param name="ContraryArguments">The contrary observations considered, retained as business-data snapshots.</param>
public sealed record ContextualPredominanceResolutionDto(
    ContextualFactorEffect Predominance,
    string ReviewerUserId,
    DateTimeOffset ResolvedAtUtc,
    string Rationale,
    IReadOnlyList<ContextualFactorEvidenceDto> FavorableArguments,
    IReadOnlyList<ContextualFactorEvidenceDto> ContraryArguments
) : IValidatableObject
{
    /// <summary>Checks that the manual resolution is attributed, dated, reasoned and identifies both argument groups.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the resolution.</param>
    /// <returns>Violations of the manual-resolution contract.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Predominance))
            yield return new ValidationResult("Predominance must be favorable, contrary or neutral.", [nameof(Predominance)]);
        if (string.IsNullOrWhiteSpace(ReviewerUserId))
            yield return new ValidationResult("A manual resolution requires a responsible reviewer.", [nameof(ReviewerUserId)]);
        if (ResolvedAtUtc == default)
            yield return new ValidationResult("A manual resolution requires its decision date.", [nameof(ResolvedAtUtc)]);
        if (string.IsNullOrWhiteSpace(Rationale))
            yield return new ValidationResult("A manual resolution requires a rationale.", [nameof(Rationale)]);

        foreach (var (name, arguments, expectedEffect) in new[]
        {
            (nameof(FavorableArguments), FavorableArguments, ContextualFactorEffect.Favorable),
            (nameof(ContraryArguments), ContraryArguments, ContextualFactorEffect.Contrary)
        })
        {
            if (arguments is null || arguments.Count == 0 || arguments.Any(argument => argument is null ||
                    string.IsNullOrWhiteSpace(argument.ReferenceCode) || argument.Effect is not { Availability: EvidenceAvailability.Present } ||
                    argument.Effect.Value != expectedEffect) ||
                arguments.Select(argument => argument.ReferenceCode).Distinct(StringComparer.Ordinal).Count() != arguments.Count)
                yield return new ValidationResult("A resolution requires distinct identified arguments in both groups.", [name]);
        }
    }
}
