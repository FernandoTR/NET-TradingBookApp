namespace Domain.Enums;

/// <summary>Identifies the family of the proposed trigger, independently of its validity.</summary>
public enum TriggerFamily
{
    /// <summary>A rejection or reversal pattern.</summary>
    Reversal = 1,

    /// <summary>A breakout, retest and continuation sequence.</summary>
    Continuation = 2
}
