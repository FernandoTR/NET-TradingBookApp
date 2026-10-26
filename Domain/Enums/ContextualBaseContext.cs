namespace Domain.Enums;

/// <summary>Describes the evidenced base context relative to the proposed trade, not a manual scenario grade.</summary>
public enum ContextualBaseContext
{
    /// <summary>The base context favors the proposed direction.</summary>
    Favorable = 1,

    /// <summary>The base context has no clear directional expectation.</summary>
    Neutral = 2,

    /// <summary>The trade opposes the contextual structure.</summary>
    Contrary = 3
}
