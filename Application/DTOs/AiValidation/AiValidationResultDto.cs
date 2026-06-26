using Domain.Enums;

namespace Application.DTOs.AiValidation;

public class AiValidationResultDto
{
    public AiValidationStatus ValidationStatus { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string PromptVersion { get; set; } = null!;

    public string SchemaVersion { get; set; } = null!;

    public string ModelResponseJson { get; set; } = null!;

    public string FinalSummary { get; set; } = null!;

    public AiVisionExtractionDto? DetectedValues { get; set; }

    public decimal? RiskRewardRatio { get; set; }

    public short? StructuralScore { get; set; }

    public int? TotalScore { get; set; }

    public string? Grade { get; set; }

    public IReadOnlyList<AiValidationRuleResultDto> Rules { get; set; } = new List<AiValidationRuleResultDto>();
}
