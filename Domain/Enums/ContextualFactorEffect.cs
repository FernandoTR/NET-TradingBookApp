namespace Domain.Enums;

/// <summary>Describes the supplied interpretation of a factor relative to the selected trade direction.</summary>
public enum ContextualFactorEffect
{
    /// <summary>The observed factor favors the proposed trade direction.</summary>
    Favorable = 1,

    /// <summary>The observed factor opposes the proposed trade direction.</summary>
    Contrary = 2,

    /// <summary>The observed factor has no clear directional effect.</summary>
    Neutral = 3
}
