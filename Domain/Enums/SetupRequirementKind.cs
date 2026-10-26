namespace Domain.Enums;

/// <summary>Distinguishes normative requirements from quality observations and additional profile filters.</summary>
public enum SetupRequirementKind
{
    /// <summary>A mandatory requirement of the strategy.</summary>
    Mandatory = 1,

    /// <summary>A quality criterion that is not by itself a mandatory graphical requirement.</summary>
    QualityCriterion = 2,

    /// <summary>An explicitly selected additional requirement of the evaluation profile.</summary>
    ProfileFilter = 3
}
