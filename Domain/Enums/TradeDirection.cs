namespace Domain.Enums;

/// <summary>Identifies the semantic trade direction independently of direction-catalog IDs.</summary>
public enum TradeDirection
{
    /// <summary>A trade in the upward price direction.</summary>
    Long = 1,

    /// <summary>A trade in the downward price direction.</summary>
    Short = 2
}
