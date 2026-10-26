namespace Domain.Enums;

/// <summary>Identifies a business element of the price map, independently of database identifiers.</summary>
public enum MapReferenceKind
{
    /// <summary>The complete high-low range of a relevant candle.</summary>
    Range = 1,

    /// <summary>A directional sequence of ranges.</summary>
    Movement = 2,

    /// <summary>A directional impulse identified in the supplied evidence.</summary>
    Impulse = 3,

    /// <summary>A structural checkpoint.</summary>
    Checkpoint = 4,

    /// <summary>A fixed checkpoint and its opposing dynamic extreme.</summary>
    Box = 5,

    /// <summary>A structural pivot, not an inferred standard candle fractal.</summary>
    Pivot = 6,

    /// <summary>A working line with its supplied influence boundaries.</summary>
    WorkingLine = 7,

    /// <summary>A temporary zone associated with a figure.</summary>
    FigureZone = 8,

    /// <summary>The complete band of a conflict between map references.</summary>
    ConflictZone = 9
}
