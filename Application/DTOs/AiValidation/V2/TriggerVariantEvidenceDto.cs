using System.Text.Json.Serialization;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Carries one of the seven alternative trigger definitions, not seven simultaneous requirements.
/// Observations describe supplied evidence, not the result of an executable graphical evaluator.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "variant")]
[JsonDerivedType(typeof(EngulfingTriggerEvidenceDto), "engulfing")]
[JsonDerivedType(typeof(StarsTriggerEvidenceDto), "stars")]
[JsonDerivedType(typeof(TweezersTriggerEvidenceDto), "tweezers")]
[JsonDerivedType(typeof(FakeOutTriggerEvidenceDto), "fakeOut")]
[JsonDerivedType(typeof(PerfectPullbackTriggerEvidenceDto), "perfectPullback")]
[JsonDerivedType(typeof(LongPullbackTriggerEvidenceDto), "longPullback")]
[JsonDerivedType(typeof(ShortPullbackTriggerEvidenceDto), "shortPullback")]
public abstract record TriggerVariantEvidenceDto;

/// <summary>Preserves common reversal observations without making movement-tip requirements universal across stages.</summary>
/// <param name="OpposesPreviousMovement">Whether the pattern opposes the preceding movement.</param>
/// <param name="ReachesMovementExtreme">Whether it reaches or extends the relevant movement tip; may be not applicable.</param>
/// <param name="RelevantRangeReferences">The exact candles or movement ranges used for the rejection-half criterion.</param>
/// <param name="RelevantRangeMidpoint">The supplied midpoint of that range, independently of the stars left-shoulder midpoint.</param>
/// <param name="RangeHalfFavorsTrade">Whether the relevant range half favors rejection and trade direction under defined criteria.</param>
public sealed record ReversalRequirementsDto(
    EvidenceValueDto<bool> OpposesPreviousMovement,
    EvidenceValueDto<bool> ReachesMovementExtreme,
    IReadOnlyList<string> RelevantRangeReferences,
    EvidenceValueDto<decimal> RelevantRangeMidpoint,
    EvidenceValueDto<bool> RangeHalfFavorsTrade
);

/// <summary>Preserves common pullback evidence without requiring a subsequent candle close.</summary>
/// <param name="HasOrderedSequence">Whether breakout, retest and continuation are evidenced in order; OHLC alone is insufficient.</param>
/// <param name="ContinuationExceedsBreakoutExtreme">Whether continuation exceeds the appropriate breakout extreme.</param>
/// <param name="TriggerFrameCandleCount">The observed number of trigger-frame candles, not the number of intrabars or closed candles.</param>
public sealed record PullbackRequirementsDto(
    EvidenceValueDto<bool> HasOrderedSequence,
    EvidenceValueDto<bool> ContinuationExceedsBreakoutExtreme,
    EvidenceValueDto<int> TriggerFrameCandleCount
);

/// <summary>Evidence for the engulfing alternative, including the accepted close-by-intention distinction.</summary>
/// <param name="Reversal">The shared reversal observations.</param>
/// <param name="LeftRangeReference">The left range against which the final close is interpreted.</param>
/// <param name="ClosesBeyondLeftRangeExtreme">Whether the final close exceeds the favorable extreme of the left high-low range.</param>
/// <param name="EnclosesEntireLeftRange">Whether the ideal full-range enclosure is observed; not a universal additional requirement.</param>
public sealed record EngulfingTriggerEvidenceDto(
    ReversalRequirementsDto Reversal,
    string LeftRangeReference,
    EvidenceValueDto<bool> ClosesBeyondLeftRangeExtreme,
    EvidenceValueDto<bool> EnclosesEntireLeftRange
) : TriggerVariantEvidenceDto;

/// <summary>Evidence for stars, preserving shoulder/head identities and the separate left-shoulder midpoint.</summary>
/// <param name="Reversal">The shared reversal observations.</param>
/// <param name="LeftShoulderReferences">The candles composing the left shoulder.</param>
/// <param name="HeadReferences">The candles composing the head, including multi-candle variants.</param>
/// <param name="RightShoulderReferences">The candles composing the right shoulder.</param>
/// <param name="HeadIsIndecisive">The supplied indecision observation; unknown if the qualitative criterion is unresolved.</param>
/// <param name="ShouldersStayWithinHeadExtreme">Whether the shoulders respect the head's relevant extreme.</param>
/// <param name="LeftShoulderMidpoint">The supplied midpoint of the left shoulder's high-low range.</param>
/// <param name="RightCloseReachesMidpoint">Whether the right close reaches the required midpoint under explicitly defined criteria.</param>
/// <param name="RightCloseBeyondMidpoint">The preferred beyond-midpoint close, distinct from the minimum criterion.</param>
public sealed record StarsTriggerEvidenceDto(
    ReversalRequirementsDto Reversal,
    IReadOnlyList<string> LeftShoulderReferences,
    IReadOnlyList<string> HeadReferences,
    IReadOnlyList<string> RightShoulderReferences,
    EvidenceValueDto<bool> HeadIsIndecisive,
    EvidenceValueDto<bool> ShouldersStayWithinHeadExtreme,
    EvidenceValueDto<decimal> LeftShoulderMidpoint,
    EvidenceValueDto<bool> RightCloseReachesMidpoint,
    EvidenceValueDto<bool> RightCloseBeyondMidpoint
) : TriggerVariantEvidenceDto;

/// <summary>Evidence for tweezers without inventing a range-similarity percentage.</summary>
/// <param name="Reversal">The shared reversal observations.</param>
/// <param name="PatternCandleReferences">The ranges composing the proposed tweezers.</param>
/// <param name="RangesAreSimilar">The supplied equality or similarity observation.</param>
/// <param name="OrderProducesReversal">Whether the observed candle order produces the proposed reversal.</param>
/// <param name="CloseFavorsRejection">Whether the closing behavior supports rejection.</param>
/// <param name="SimilarityTolerance">A price tolerance only if explicitly supplied; unknown when no criterion is defined.</param>
public sealed record TweezersTriggerEvidenceDto(
    ReversalRequirementsDto Reversal,
    IReadOnlyList<string> PatternCandleReferences,
    EvidenceValueDto<bool> RangesAreSimilar,
    EvidenceValueDto<bool> OrderProducesReversal,
    EvidenceValueDto<bool> CloseFavorsRejection,
    EvidenceValueDto<decimal> SimilarityTolerance
) : TriggerVariantEvidenceDto;

/// <summary>Evidence for a fake out, distinguishing an actual break from merely approaching a reference.</summary>
/// <param name="Reversal">The shared reversal observations.</param>
/// <param name="ReferenceBounds">The supplied influence boundaries at the relevant break moment.</param>
/// <param name="BreakDefinitionCode">The explicitly chosen trace/band interpretation, if defined.</param>
/// <param name="ReferenceWasBroken">Whether a break is observed under that interpretation.</param>
/// <param name="BreakWasRejected">Whether the actual break is subsequently rejected.</param>
public sealed record FakeOutTriggerEvidenceDto(
    ReversalRequirementsDto Reversal,
    ZoneInfluenceBoundsDto ReferenceBounds,
    string? BreakDefinitionCode,
    EvidenceValueDto<bool> ReferenceWasBroken,
    EvidenceValueDto<bool> BreakWasRejected
) : TriggerVariantEvidenceDto;

/// <summary>Evidence for a perfect pullback with explicit reference contact.</summary>
/// <param name="Pullback">The shared ordered-sequence and continuation observations.</param>
/// <param name="RetestTouchesReference">Whether the retest contacts the relevant working reference.</param>
/// <param name="ContactTolerance">A price tolerance only when explicitly supplied, without a universal default.</param>
public sealed record PerfectPullbackTriggerEvidenceDto(
    PullbackRequirementsDto Pullback,
    EvidenceValueDto<bool> RetestTouchesReference,
    EvidenceValueDto<decimal> ContactTolerance
) : TriggerVariantEvidenceDto;

/// <summary>Evidence for a long pullback, preserving the retest midpoint rather than substituting an impulse Fibonacci level.</summary>
/// <param name="Pullback">The shared ordered-sequence and continuation observations.</param>
/// <param name="RetestCrossesReference">Whether the retest crosses the working reference.</param>
/// <param name="RetestMidpoint">The retest midpoint with the moment to which the measurement applies.</param>
/// <param name="MidpointRemainsOnFavorableSide">Whether that midpoint respects the favorable-side criterion.</param>
public sealed record LongPullbackTriggerEvidenceDto(
    PullbackRequirementsDto Pullback,
    EvidenceValueDto<bool> RetestCrossesReference,
    PriceTimeAnchorDto RetestMidpoint,
    EvidenceValueDto<bool> MidpointRemainsOnFavorableSide
) : TriggerVariantEvidenceDto;

/// <summary>Evidence for a short pullback, whose retest can remain before the working reference.</summary>
/// <param name="Pullback">The shared ordered-sequence and continuation observations.</param>
/// <param name="RetestRemainsBeforeReference">Whether the retest stops before reaching the reference.</param>
/// <param name="DistanceToReference">The supplied distance between retest extreme and reference at the relevant moment.</param>
/// <param name="RetestAmplitude">The supplied amplitude of the retest used for the distance comparison.</param>
public sealed record ShortPullbackTriggerEvidenceDto(
    PullbackRequirementsDto Pullback,
    EvidenceValueDto<bool> RetestRemainsBeforeReference,
    EvidenceValueDto<decimal> DistanceToReference,
    EvidenceValueDto<decimal> RetestAmplitude
) : TriggerVariantEvidenceDto;
