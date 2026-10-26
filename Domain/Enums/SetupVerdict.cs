namespace Domain.Enums;

/// <summary>Describes graphical validity independently of score and real-trading review.</summary>
public enum SetupVerdict
{
    /// <summary>Graphical validity has not yet been determined.</summary>
    Pending = 0,

    /// <summary>All three mandatory graphical conditions are satisfied.</summary>
    Valid = 1,

    /// <summary>At least one mandatory graphical condition is not satisfied.</summary>
    Invalid = 2
}
