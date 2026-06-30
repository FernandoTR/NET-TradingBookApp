using System;
using System.Collections.Generic;

namespace Infrastructure;

public partial class AiTradeValidation
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int? OrderId { get; set; }

    public int InstrumentId { get; set; }

    public int DirectionId { get; set; }

    public decimal EntryPrice { get; set; }

    public decimal StopLoss { get; set; }

    public decimal TakeProfit { get; set; }

    public string? UserComment { get; set; }

    public int? DetectedTriggerId { get; set; }

    public int? DetectedSceneryId { get; set; }

    public int? DetectedFigureId { get; set; }

    public int? DetectedFrameId { get; set; }

    public int? DetectedStageId { get; set; }

    public byte? DetectedLocationType { get; set; }

    public byte? DetectedConfirmationType { get; set; }

    public bool? DetectedIsTrendAligned { get; set; }

    public bool? DetectedIsPivotZone { get; set; }

    public int? ConfirmedTriggerId { get; set; }

    public int? ConfirmedSceneryId { get; set; }

    public int? ConfirmedFigureId { get; set; }

    public int? ConfirmedFrameId { get; set; }

    public int? ConfirmedStageId { get; set; }

    public byte? ConfirmedLocationType { get; set; }

    public byte? ConfirmedConfirmationType { get; set; }

    public bool? ConfirmedIsTrendAligned { get; set; }

    public bool? ConfirmedIsPivotZone { get; set; }

    public decimal? RiskRewardRatio { get; set; }

    public short? StructuralScore { get; set; }

    public int? TotalScore { get; set; }

    public string? Grade { get; set; }

    public decimal? VisualConfidence { get; set; }

    public string ValidationStatus { get; set; } = null!;

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string PromptVersion { get; set; } = null!;

    public string SchemaVersion { get; set; } = null!;

    public string ModelResponseJson { get; set; } = null!;

    public string FinalSummary { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public virtual AiTradeValidationMetric? Metric { get; set; }

    public virtual ICollection<AiTradeValidationRule> Rules { get; set; } = new List<AiTradeValidationRule>();
}
