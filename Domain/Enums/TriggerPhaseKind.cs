namespace Domain.Enums;

/// <summary>Identifies a phase of a supplied pullback sequence without inferring its chronological order.</summary>
public enum TriggerPhaseKind
{
    /// <summary>The breakout of the relevant map reference.</summary>
    Breakout = 1,

    /// <summary>The retest following a breakout.</summary>
    Retest = 2,

    /// <summary>The continuation following the retest.</summary>
    Continuation = 3
}
