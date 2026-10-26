namespace Application.DTOs.AiValidation.V2;

/// <summary>Associates a new contextual calculation with its order while preserving the legacy result and manual Scenery.</summary>
/// <param name="OrderId">The source order ID, or null for an order not yet persisted.</param>
/// <param name="ManualSceneryId">The manual classification, retained outside the calculation input.</param>
/// <param name="OriginalScore">The original legacy score, unchanged by this calculation.</param>
/// <param name="Evaluation">The independently versioned contextual evaluation.</param>
public sealed record OrderContextualScoreEvaluationDto(
    int? OrderId,
    int ManualSceneryId,
    LegacyTradingScoreDto OriginalScore,
    ContextualScoreEvaluationDto Evaluation
);
