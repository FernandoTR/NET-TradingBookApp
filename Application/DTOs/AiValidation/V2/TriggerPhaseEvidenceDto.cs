using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Represents supplied evidence of a breakout, retest or continuation.
/// List order and candle color do not establish chronology; restarted sequences retain distinct business references.
/// </summary>
/// <param name="ReferenceCode">The business reference identifying this phase.</param>
/// <param name="SequenceReferenceCode">The sequence this phase belongs to, including a new sequence after invalidation.</param>
/// <param name="Kind">The observed phase of the trigger.</param>
/// <param name="SequenceIndex">The observed chronological position, or unknown when order cannot be established.</param>
/// <param name="ZoneReferenceCode">The map zone against which the phase is interpreted.</param>
/// <param name="ReferenceBounds">The supplied zone boundaries at the moment relevant to this phase.</param>
/// <param name="Start">The supplied price and time at the phase start.</param>
/// <param name="End">The supplied price and time at the phase end.</param>
/// <param name="Extreme">The relevant phase extreme with its own provenance and availability.</param>
/// <param name="CandleReferences">The candles supporting this phase; an empty list does not prove absence.</param>
/// <param name="IsComplete">Whether phase completion is observed, independently of candle closure.</param>
public sealed record TriggerPhaseEvidenceDto(
    string ReferenceCode,
    string SequenceReferenceCode,
    EvidenceValueDto<TriggerPhaseKind> Kind,
    EvidenceValueDto<int> SequenceIndex,
    string ZoneReferenceCode,
    ZoneInfluenceBoundsDto ReferenceBounds,
    PriceTimeAnchorDto Start,
    PriceTimeAnchorDto End,
    PriceTimeAnchorDto Extreme,
    IReadOnlyList<string> CandleReferences,
    EvidenceValueDto<bool> IsComplete
);
