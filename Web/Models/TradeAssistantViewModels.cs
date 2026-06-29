using Application.DTOs.AiValidation;

namespace Web.Models;

public sealed class TradeAssistantCreateViewModel
{
    public int InstrumentId { get; set; }

    public int DirectionId { get; set; }

    public decimal EntryPrice { get; set; }

    public decimal StopLoss { get; set; }

    public decimal TakeProfit { get; set; }

    public int? FrameId { get; set; }

    public int? SceneryId { get; set; }

    public int? StageId { get; set; }

    public int? TriggerId { get; set; }

    public int? FigureId { get; set; }

    public string? UserComment { get; set; }

    public List<TradeAssistantImageViewModel> Images { get; set; } = new();
}

public sealed class TradeAssistantImageViewModel
{
    public IFormFile File { get; set; } = null!;

    public int ImageRole { get; set; }

    public int? FrameId { get; set; }

    public int SortOrder { get; set; }

    public string? Comment { get; set; }
}

public sealed class TradeAssistantResultViewModel
{
    public int ValidationId { get; set; }

    public AiValidationResultDto Result { get; set; } = null!;

    public ConfirmedAiValidationDto Confirmation { get; set; } = null!;
}

public sealed class TradeAssistantHistoryItemViewModel
{
    public int ValidationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Instrument { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public string ValidationStatus { get; set; } = string.Empty;

    public int? TotalScore { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;
}
