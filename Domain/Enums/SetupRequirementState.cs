namespace Domain.Enums;

/// <summary>Describes an individual requirement, including justified inapplicability.</summary>
public enum SetupRequirementState
{
    /// <summary>The requirement has not yet been determined.</summary>
    Pending = 0,

    /// <summary>The requirement is satisfied.</summary>
    Passed = 1,

    /// <summary>The requirement is not satisfied.</summary>
    Failed = 2,

    /// <summary>The requirement does not apply to this case, with an explicit reason.</summary>
    NotApplicable = 3
}
