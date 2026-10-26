namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Preserves the proposed technical risk plan without selecting a stop, bypassing obstacles or evaluating the ratio.
/// Unknown prices and margins remain unknown rather than being filled from monetary-risk preferences.
/// </summary>
/// <param name="Entry">The proposed entry price and its business reference moment.</param>
/// <param name="StopLoss">The supplied stop-loss price, separate from the underlying technical invalidation.</param>
/// <param name="TechnicalInvalidation">The supplied trigger/zone invalidation price and moment.</param>
/// <param name="InvalidationEvidenceReferences">The pattern extremes or zone references supporting that invalidation.</param>
/// <param name="TechnicalMargin">The explicitly supplied margin in price units, without a universal default.</param>
/// <param name="FirstObstacle">The first relevant map obstacle assessment, independently of the desired target.</param>
/// <param name="Target">The proposed target price and moment; a distant target does not remove a nearer obstacle.</param>
public sealed record RiskPlanEvidenceDto(
    PriceTimeAnchorDto Entry,
    PriceTimeAnchorDto StopLoss,
    PriceTimeAnchorDto TechnicalInvalidation,
    IReadOnlyList<string> InvalidationEvidenceReferences,
    EvidenceValueDto<decimal> TechnicalMargin,
    RiskObstacleEvidenceDto FirstObstacle,
    PriceTimeAnchorDto Target
);
