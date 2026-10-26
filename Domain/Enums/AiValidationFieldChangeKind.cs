namespace Domain.Enums;

/// <summary>Classifies a human review without turning an unknown observation into a field match.</summary>
public enum AiValidationFieldChangeKind
{
    /// <summary>The reviewed human value agrees with the AI value, including confirmed absence versus absence.</summary>
    NoChange = 1,

    /// <summary>The reviewed human value differs from a known AI assertion.</summary>
    Correction = 2,

    /// <summary>The user supplied a known value for an AI-unknown observation.</summary>
    ManualCompletion = 3,

    /// <summary>The user reviewed the field but could not determine it, so no forced comparison is made.</summary>
    Discarded = 4,

    /// <summary>No human review is recorded for this field.</summary>
    NoReview = 5
}
