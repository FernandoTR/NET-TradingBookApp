namespace Domain.Enums;

/// <summary>Identifies verified figure concepts independently of catalog IDs and contextual grades.</summary>
public enum FigureConceptKind
{
    /// <summary>The pivot-line figure, independently of the IsPivotZone observation.</summary>
    PivotLine = 1,

    /// <summary>An ascending channel.</summary>
    AscendingChannel = 2,

    /// <summary>A descending channel.</summary>
    DescendingChannel = 3,

    /// <summary>A neutral channel.</summary>
    NeutralChannel = 4,

    /// <summary>An ascending triangle.</summary>
    AscendingTriangle = 5,

    /// <summary>A descending triangle.</summary>
    DescendingTriangle = 6,

    /// <summary>A symmetric triangle.</summary>
    SymmetricTriangle = 7,

    /// <summary>A cup-and-handle figure.</summary>
    CupAndHandle = 8,

    /// <summary>A trend line, distinct from the pivot-line figure.</summary>
    TrendLine = 9
}
