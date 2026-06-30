namespace Application.DTOs.AiValidation;

public class AiValidationMetricDto
{
    public int Id { get; set; }

    public int ValidationId { get; set; }

    public int? OrderId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public bool? TriggerMatchedUser { get; set; }

    public bool? SceneryMatchedUser { get; set; }

    public bool? StageMatchedUser { get; set; }

    public bool? FigureMatchedUser { get; set; }

    public bool? FrameMatchedUser { get; set; }

    public bool? DirectionMatchedUser { get; set; }

    public bool? TrendMatchedUser { get; set; }

    public bool? LocationMatchedUser { get; set; }

    public bool? ConfirmationMatchedUser { get; set; }

    public bool? PivotZoneMatchedUser { get; set; }

    public decimal HumanCorrectionRate { get; set; }

    public int? TotalScore { get; set; }

    public string? Grade { get; set; }

    public bool? ReachedSl { get; set; }

    public bool? ReachedTp1 { get; set; }

    public bool? ReachedTp2 { get; set; }

    public bool? ReachedTp3 { get; set; }

    public string? OutcomeClassification { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
