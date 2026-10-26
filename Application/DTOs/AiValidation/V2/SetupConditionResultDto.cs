using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Records a zone, trigger or ratio result, independently of technical provider errors and contextual score.</summary>
/// <param name="Condition">The mandatory graphical condition represented by this result.</param>
public sealed record SetupConditionResultDto(SetupConditionKind Condition) : IValidatableObject
{
    /// <summary>The condition's three-state result; the main condition cannot be marked not applicable.</summary>
    public SetupConditionState State { get; init; } = SetupConditionState.Pending;

    /// <summary>The individual mandatory requirements, quality criteria and profile filters considered.</summary>
    public IReadOnlyList<SetupRequirementResultDto> Requirements { get; init; } = [];

    /// <summary>The normative document, section and rule codes supporting the evaluation.</summary>
    public IReadOnlyList<string> NormativeReferences { get; init; } = [];

    /// <summary>The reasons for passing, failing or remaining pending.</summary>
    public IReadOnlyList<string> ReasonCodes { get; init; } = [];

    /// <summary>The references to the evidence observations actually used.</summary>
    public IReadOnlyList<string> EvidenceReferences { get; init; } = [];

    /// <summary>Checks the condition identity and its three-state result without evaluating graphical evidence.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the condition.</param>
    /// <returns>Violations of the condition-result contract.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Condition))
        {
            yield return new ValidationResult("Condition kind must be zone, trigger or ratio.", [nameof(Condition)]);
        }

        if (!Enum.IsDefined(State))
        {
            yield return new ValidationResult("A main condition must be pending, passed or failed.", [nameof(State)]);
        }

        if (Requirements is null)
        {
            yield return new ValidationResult("The requirement collection must be explicit.", [nameof(Requirements)]);
            yield break;
        }

        for (var index = 0; index < Requirements.Count; index++)
        {
            var requirement = Requirements[index];
            if (requirement is null)
            {
                yield return new ValidationResult("A requirement result must not be null.", [$"{nameof(Requirements)}[{index}]"]);
                continue;
            }

            foreach (var error in requirement.Validate(new ValidationContext(requirement)))
            {
                yield return new ValidationResult(
                    error.ErrorMessage,
                    error.MemberNames.Select(member => $"{nameof(Requirements)}[{index}].{member}").ToArray());
            }
        }
    }
}
