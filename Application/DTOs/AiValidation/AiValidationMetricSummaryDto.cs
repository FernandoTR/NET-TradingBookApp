namespace Application.DTOs.AiValidation;

public class AiValidationMetricSummaryDto
{
    public int SampleSize { get; set; }

    public decimal AverageHumanCorrectionRate { get; set; }

    public IReadOnlyList<AiValidationMetricSummaryGroupDto> Groups { get; set; } = [];

    public IReadOnlyList<AiValidationMetricDto> Metrics { get; set; } = [];
}

public class AiValidationMetricSummaryGroupDto
{
    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public int SampleSize { get; set; }

    public decimal? TriggerPrecision { get; set; }

    public decimal? SceneryPrecision { get; set; }

    public decimal? StagePrecision { get; set; }

    public decimal? FigurePrecision { get; set; }

    public decimal? FramePrecision { get; set; }

    public decimal? DirectionPrecision { get; set; }

    public decimal? TrendPrecision { get; set; }

    public decimal? LocationPrecision { get; set; }

    public decimal? ConfirmationPrecision { get; set; }

    public decimal? PivotZonePrecision { get; set; }

    public decimal AverageHumanCorrectionRate { get; set; }

    public IReadOnlyList<AiValidationMetricGradeOutcomeDto> GradeOutcomes { get; set; } = [];
}

public class AiValidationMetricGradeOutcomeDto
{
    public string? Grade { get; set; }

    public string OutcomeClassification { get; set; } = null!;

    public int Count { get; set; }

    public decimal Percentage { get; set; }
}
