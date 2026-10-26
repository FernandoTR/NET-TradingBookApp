namespace Domain.Enums;

/// <summary>Identifies one of the three mandatory graphical conditions.</summary>
public enum SetupConditionKind
{
    /// <summary>A working zone and an admissible trigger timeframe.</summary>
    Zone = 1,

    /// <summary>A trigger meeting its specific variant requirements.</summary>
    Trigger = 2,

    /// <summary>The required ratio with a technical stop and available path.</summary>
    Ratio = 3
}
