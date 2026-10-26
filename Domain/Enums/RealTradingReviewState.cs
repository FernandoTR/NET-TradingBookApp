namespace Domain.Enums;

/// <summary>Records a manual real-trading review independently of graphical validity and contextual score.</summary>
public enum RealTradingReviewState
{
    /// <summary>No real-trading decision has been recorded.</summary>
    Pending = 0,

    /// <summary>A reviewer has explicitly enabled real trading for the referenced evaluation revision.</summary>
    Enabled = 1,

    /// <summary>A reviewer has explicitly not enabled real trading for the referenced evaluation revision.</summary>
    NotEnabled = 2
}
