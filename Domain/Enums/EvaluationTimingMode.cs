namespace Domain.Enums;

/// <summary>Identifies the temporal modality explicitly selected for a setup.</summary>
public enum EvaluationTimingMode
{
    /// <summary>The setup is considered at candle close.</summary>
    ClosedCandle = 1,

    /// <summary>The setup is considered within a candle, using the available sequence evidence.</summary>
    Intrabar = 2,

    /// <summary>The setup anticipates a candle close rather than claiming a confirmed closing pattern.</summary>
    AnticipatedClose = 3
}
