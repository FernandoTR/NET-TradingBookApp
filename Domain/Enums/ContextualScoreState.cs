namespace Domain.Enums;

/// <summary>Describes the availability of a contextual classification and its ordinal score.</summary>
public enum ContextualScoreState
{
    /// <summary>The base context required for classification cannot be established.</summary>
    NotEvaluable = 0,

    /// <summary>Some context is known, but required evidence is still missing.</summary>
    Partial = 1,

    /// <summary>The classification is determinable and all required evidence is available.</summary>
    Complete = 2
}
