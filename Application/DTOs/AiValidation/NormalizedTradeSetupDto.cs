namespace Application.DTOs.AiValidation;

public sealed class NormalizedTradeSetupDto
{
    public int InstrumentId { get; set; }

    public int DirectionId { get; set; }

    public decimal EntryPrice { get; set; }

    public decimal StopLoss { get; set; }

    public decimal TakeProfit { get; set; }

    public int? TriggerId { get; set; }

    public int? SceneryId { get; set; }

    public int? FigureId { get; set; }

    public int? FrameId { get; set; }

    public int? StageId { get; set; }

    public byte? LocationType { get; set; }

    public byte? ConfirmationType { get; set; }

    public bool? IsTrendAligned { get; set; }

    public bool? IsPivotZone { get; set; }

    public decimal? RiskRewardRatio { get; set; }

    public decimal VisualConfidence { get; set; }
}
