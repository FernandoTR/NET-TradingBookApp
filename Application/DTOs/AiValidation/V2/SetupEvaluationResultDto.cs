using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Composes three supplied condition results into graphical validity without executing their evaluators.
/// A graphical result neither computes a contextual score nor enables real trading.
/// </summary>
/// <param name="EvaluationReferenceCode">The business identity of this evaluation, independently of future persistence IDs.</param>
/// <param name="EvidenceReferenceCode">The evidence document or snapshot used for this evaluation.</param>
/// <param name="Versions">The recorded versions used, preserving unknown source versions.</param>
public sealed record SetupEvaluationResultDto(
    string EvaluationReferenceCode,
    string EvidenceReferenceCode,
    SetupEvaluationVersionsDto Versions
) : IValidatableObject
{
    /// <summary>The source evaluation revision, when known; legacy sources do not receive an invented revision.</summary>
    public int? EvaluationRevision { get; init; }

    /// <summary>When the evaluation occurred, when recorded.</summary>
    public DateTimeOffset? EvaluatedAtUtc { get; init; }

    /// <summary>The zone result, pending until explicitly evaluated.</summary>
    public SetupConditionResultDto Zone { get; init; } = new(SetupConditionKind.Zone);

    /// <summary>The trigger result, pending until explicitly evaluated.</summary>
    public SetupConditionResultDto Trigger { get; init; } = new(SetupConditionKind.Trigger);

    /// <summary>The ratio result, pending until explicitly evaluated.</summary>
    public SetupConditionResultDto Ratio { get; init; } = new(SetupConditionKind.Ratio);

    /// <summary>Invalid if any condition fails, otherwise pending unless all three conditions pass.</summary>
    public SetupVerdict Verdict
    {
        get
        {
            if (Zone?.State == SetupConditionState.Failed ||
                Trigger?.State == SetupConditionState.Failed ||
                Ratio?.State == SetupConditionState.Failed)
            {
                return SetupVerdict.Invalid;
            }

            return Zone is { Condition: SetupConditionKind.Zone, State: SetupConditionState.Passed } &&
                Trigger is { Condition: SetupConditionKind.Trigger, State: SetupConditionState.Passed } &&
                Ratio is { Condition: SetupConditionKind.Ratio, State: SetupConditionState.Passed }
                    ? SetupVerdict.Valid
                    : SetupVerdict.Pending;
        }
    }

    /// <summary>Checks evidence linkage, revision and each condition's identity and state.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the evaluation.</param>
    /// <returns>Violations of the evaluation-result contract.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EvaluationReferenceCode))
        {
            yield return new ValidationResult("An evaluation requires a business reference.", [nameof(EvaluationReferenceCode)]);
        }

        if (string.IsNullOrWhiteSpace(EvidenceReferenceCode))
        {
            yield return new ValidationResult("An evaluation requires an evidence reference.", [nameof(EvidenceReferenceCode)]);
        }

        if (Versions is null)
        {
            yield return new ValidationResult("Recorded versions must be explicit, including unknown values.", [nameof(Versions)]);
        }

        if (EvaluationRevision is <= 0)
        {
            yield return new ValidationResult("A known evaluation revision must be positive.", [nameof(EvaluationRevision)]);
        }

        foreach (var (name, expected, result) in new[]
        {
            (nameof(Zone), SetupConditionKind.Zone, Zone),
            (nameof(Trigger), SetupConditionKind.Trigger, Trigger),
            (nameof(Ratio), SetupConditionKind.Ratio, Ratio)
        })
        {
            if (result is null || result.Condition != expected)
            {
                yield return new ValidationResult("Each graphical condition requires its own correctly identified result.", [name]);
                continue;
            }

            foreach (var error in result.Validate(new ValidationContext(result)))
            {
                yield return new ValidationResult(error.ErrorMessage, error.MemberNames.Select(member => $"{name}.{member}"));
            }
        }
    }
}
