namespace Domain.Enums;

/// <summary>Indicates whether an observation is known, absent or inapplicable.</summary>
public enum EvidenceAvailability
{
    /// <summary>The observation cannot be determined.</summary>
    Unknown = 0,

    /// <summary>A value is known, including false or zero.</summary>
    Present = 1,

    /// <summary>The absence of the observed element has been confirmed.</summary>
    Absent = 2,

    /// <summary>The requirement does not apply to this case.</summary>
    NotApplicable = 3
}
