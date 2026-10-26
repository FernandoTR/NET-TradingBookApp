using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Identifies one comparable source population and its independently recorded provider/definition versions.</summary>
public sealed record AiValidationMetricVersionGroupV2Dto(
    MetricPopulationKind PopulationKind,
    string? ProviderName,
    string? ModelName,
    string? StrategyVersion,
    string? EvidenceSchemaVersion,
    string? GraphicalRulesVersion,
    string? ClassificationRulesVersion,
    string? ScoreVersion,
    string? MetricsVersion,
    string? PromptVersion,
    string? ExtractionSchemaVersion
);
