namespace Application.DTOs.AiValidation.V2;

/// <summary>Combines the versioned business evidence used at one evaluation moment.</summary>
/// <param name="StrategyVersion">The strategy definition applied to this evidence document.</param>
/// <param name="EvidenceSchemaVersion">The schema defining this evidence document, independently of a provider's extraction schema.</param>
/// <param name="EvaluatedAtUtc">When this evaluation occurs, independently of fact occurrence and information availability.</param>
/// <param name="Context">The instrument, direction, stage, trigger frame and temporal modality.</param>
/// <param name="Zone">The selected working zone and its supplied evidence.</param>
/// <param name="Trigger">The proposed trigger and its variant-specific observations.</param>
/// <param name="RiskPlan">The proposed technical risk plan and first-obstacle assessment.</param>
/// <param name="ContextualEvidence">The figure, structures, convergences and contextual factors.</param>
/// <param name="Conflicts">Contradictory observations and separately recorded manual resolutions.</param>
public sealed record TradeSetupEvidenceDto(
    string StrategyVersion,
    string EvidenceSchemaVersion,
    DateTimeOffset EvaluatedAtUtc,
    EvaluationContextDto Context,
    ZoneEvidenceDto Zone,
    TriggerEvidenceDto Trigger,
    RiskPlanEvidenceDto RiskPlan,
    ContextualEvidenceDto ContextualEvidence,
    IReadOnlyList<EvidenceConflictDto> Conflicts
);
