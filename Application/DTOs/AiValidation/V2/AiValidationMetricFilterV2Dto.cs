using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Filters one user's metric snapshots by population, recorded dimensions, versions and capture time.</summary>
/// <param name="PopulationKind">The historical-order or reviewed-evaluation population, or both when null.</param>
/// <param name="ProviderName">An exact provider filter when recorded.</param>
/// <param name="ModelName">An exact model filter when recorded.</param>
/// <param name="FromUtc">The inclusive snapshot-capture start.</param>
/// <param name="ToUtc">The inclusive snapshot-capture end.</param>
/// <param name="InstrumentId">A recorded source instrument filter.</param>
/// <param name="StageId">A recorded source stage filter.</param>
/// <param name="TriggerId">A recorded source trigger filter.</param>
/// <param name="StrategyVersion">An exact strategy-version filter; unknown versions remain null.</param>
/// <param name="EvidenceSchemaVersion">An exact evidence-schema-version filter.</param>
/// <param name="GraphicalRulesVersion">An exact graphical-rules-version filter.</param>
/// <param name="ClassificationRulesVersion">An exact classification-rules-version filter.</param>
/// <param name="ScoreVersion">An exact score-version filter.</param>
/// <param name="MetricsVersion">An exact metric-version filter.</param>
/// <param name="PromptVersion">An exact source prompt-version filter.</param>
/// <param name="ExtractionSchemaVersion">An exact provider extraction-schema filter.</param>
/// <param name="IncludeAllRevisions">Whether to include every recorded validation revision instead of the latest by default.</param>
public sealed record AiValidationMetricFilterV2Dto(
    MetricPopulationKind? PopulationKind = null,
    string? ProviderName = null,
    string? ModelName = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null,
    int? InstrumentId = null,
    int? StageId = null,
    int? TriggerId = null,
    string? StrategyVersion = null,
    string? EvidenceSchemaVersion = null,
    string? GraphicalRulesVersion = null,
    string? ClassificationRulesVersion = null,
    string? ScoreVersion = null,
    string? MetricsVersion = null,
    string? PromptVersion = null,
    string? ExtractionSchemaVersion = null,
    bool IncludeAllRevisions = false
);
