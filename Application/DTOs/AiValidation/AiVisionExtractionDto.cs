using Domain.Enums;

namespace Application.DTOs.AiValidation;

public class AiVisionExtractionDto
{
    public int? TriggerId { get; set; }

    public int? SceneryId { get; set; }

    public int? FigureId { get; set; }

    public int? FrameId { get; set; }

    public int? StageId { get; set; }

    public LocationType? LocationType { get; set; }

    public ConfirmationType? ConfirmationType { get; set; }

    public bool? IsTrendAligned { get; set; }

    public bool? IsPivotZone { get; set; }

    public decimal? VisualConfidence { get; set; }
}
