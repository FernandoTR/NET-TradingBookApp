using Domain.Enums;

namespace Domain.Entities;

/// <summary>Persists one immutable, population-identified metric review independently of its original validation.</summary>
public sealed class AiTradeValidationMetricSnapshot
{
    public int Id { get; set; }

    public MetricPopulationKind PopulationKind { get; set; }

    public string UserId { get; set; } = null!;

    public int? OrderId { get; set; }

    public int? ValidationId { get; set; }

    public int? EvaluationRevision { get; set; }

    public int? ContextualScoreSnapshotId { get; set; }

    public int? InstrumentId { get; set; }

    public int? StageId { get; set; }

    public int? TriggerId { get; set; }

    public string? StrategyVersion { get; set; }

    public string? EvidenceSchemaVersion { get; set; }

    public string? GraphicalRulesVersion { get; set; }

    public string? ClassificationRulesVersion { get; set; }

    public string? ScoreVersion { get; set; }

    public string? MetricsVersion { get; set; }

    public string? PromptVersion { get; set; }

    public string? ExtractionSchemaVersion { get; set; }

    public string? ProviderName { get; set; }

    public string? ModelName { get; set; }

    public string FieldsJson { get; set; } = null!;

    public string? GraphicResultsJson { get; set; }

    public string? OutcomeObservationsJson { get; set; }

    public DateTimeOffset CapturedAtUtc { get; set; }
}
