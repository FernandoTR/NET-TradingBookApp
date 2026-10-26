namespace Domain.Enums;

/// <summary>Describes the evaluation of a mandatory graphical condition.</summary>
public enum SetupConditionState
{
    /// <summary>The condition has not yet been determined.</summary>
    Pending = 0,

    /// <summary>The condition is satisfied.</summary>
    Passed = 1,

    /// <summary>The condition is not satisfied.</summary>
    Failed = 2
}
