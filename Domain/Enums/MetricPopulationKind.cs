namespace Domain.Enums;

/// <summary>Identifies the population represented by one immutable metric snapshot.</summary>
public enum MetricPopulationKind : byte
{
    /// <summary>A historical order without an AI-to-user review population.</summary>
    HistoricalOrder = 1,

    /// <summary>An AI validation with an explicit review population.</summary>
    ReviewedEvaluation = 2
}
