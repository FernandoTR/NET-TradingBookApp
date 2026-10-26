using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Records the result of one identified requirement, quality criterion or profile filter.</summary>
/// <param name="RequirementCode">The rule or criterion being considered.</param>
/// <param name="Kind">Whether the criterion is mandatory, qualitative or an additional profile filter.</param>
public sealed record SetupRequirementResultDto(string RequirementCode, SetupRequirementKind Kind) : IValidatableObject
{
    /// <summary>The requirement result, pending until explicitly evaluated.</summary>
    public SetupRequirementState State { get; init; } = SetupRequirementState.Pending;

    /// <summary>The normative document, section or rule codes supporting this criterion.</summary>
    public IReadOnlyList<string> NormativeReferences { get; init; } = [];

    /// <summary>The reasons for the outcome, including any justified inapplicability.</summary>
    public IReadOnlyList<string> ReasonCodes { get; init; } = [];

    /// <summary>References to the observations used for this requirement, not file metadata.</summary>
    public IReadOnlyList<string> EvidenceReferences { get; init; } = [];

    /// <summary>Checks result states and requires an identifiable reason for inapplicability.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the result.</param>
    /// <returns>Violations of the requirement-result contract.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(RequirementCode))
        {
            yield return new ValidationResult("A requirement requires an identifiable code.", [nameof(RequirementCode)]);
        }

        if (!Enum.IsDefined(Kind))
        {
            yield return new ValidationResult("Requirement kind must be a defined value.", [nameof(Kind)]);
        }

        if (!Enum.IsDefined(State))
        {
            yield return new ValidationResult("Requirement state must be a defined value.", [nameof(State)]);
        }

        if (State == SetupRequirementState.NotApplicable &&
            (ReasonCodes is null || !ReasonCodes.Any(reason => !string.IsNullOrWhiteSpace(reason))))
        {
            yield return new ValidationResult("An inapplicable requirement requires an identifiable reason.", [nameof(ReasonCodes)]);
        }
    }
}
