namespace Domain.Enums;

/// <summary>Describes an observed structure independently of the requested Long/Short trade direction.</summary>
public enum StructureDirection
{
    /// <summary>The structure is ascending.</summary>
    Upward = 1,

    /// <summary>The structure is descending.</summary>
    Downward = 2,

    /// <summary>The structure has no clear directional expectation.</summary>
    Neutral = 3
}
