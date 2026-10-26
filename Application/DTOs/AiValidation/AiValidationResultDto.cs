using Application.DTOs.AiValidation.V2;
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

    public HistoricalEvidenceDto? HistoricalEvidence { get; set; }

    public decimal? RiskRewardRatio { get; set; }

    public short? StructuralScore { get; set; }

    public int? TotalScore { get; set; }

    public string? Grade { get; set; }

    /// <summary>The independently versioned contextual calculations, retaining the original and subsequent recalculations.</summary>
    public IReadOnlyList<ContextualScoreEvaluationDto> ContextualEvaluations { get; set; } = [];

    /// <summary>The current contextual calculation, with its decimal score and availability separate from legacy integer fields.</summary>
    public ContextualScoreEvaluationDto? ContextualEvaluation => ContextualEvaluations.LastOrDefault();

    public IReadOnlyList<AiValidationRuleResultDto> Rules { get; set; } = new List<AiValidationRuleResultDto>();
}
