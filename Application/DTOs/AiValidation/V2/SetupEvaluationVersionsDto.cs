namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Identifies independently versioned definitions and provider artifacts used for a result.
/// Unknown or unused versions remain null; a recalculation does not rewrite the source evaluation's versions.
/// </summary>
/// <param name="StrategyVersion">The strategy version, when recorded.</param>
/// <param name="EvidenceSchemaVersion">The business-evidence schema version, when recorded.</param>
/// <param name="GraphicalRulesVersion">The rules used to evaluate zone, trigger and ratio, when recorded.</param>
/// <param name="ClassificationRulesVersion">The contextual-classification rules, when used and recorded.</param>
/// <param name="ScoreVersion">The score definition, when used and recorded.</param>
/// <param name="MetricsVersion">The metric definition, when used and recorded.</param>
/// <param name="PromptVersion">The source provider prompt version, when recorded.</param>
/// <param name="ExtractionSchemaVersion">The source provider extraction schema, independently of the business-evidence schema.</param>
public sealed record SetupEvaluationVersionsDto(
    string? StrategyVersion = null,
    string? EvidenceSchemaVersion = null,
    string? GraphicalRulesVersion = null,
    string? ClassificationRulesVersion = null,
    string? ScoreVersion = null,
    string? MetricsVersion = null,
    string? PromptVersion = null,
    string? ExtractionSchemaVersion = null
);
