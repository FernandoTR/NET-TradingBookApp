namespace Domain.Enums;

/// <summary>Identifies the source population of a contextual score snapshot.</summary>
public enum ContextualScoreSourceKind : byte
{
    /// <summary>A legacy order evaluated from information that was already recorded.</summary>
    HistoricalOrder = 1,

    /// <summary>A separately identified, reviewed setup evaluation.</summary>
    ReviewedEvaluation = 2
}
