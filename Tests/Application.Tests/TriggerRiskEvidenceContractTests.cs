using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.DTOs.AiValidation.V2;
using Domain.Enums;

namespace Application.Tests;

public class TriggerRiskEvidenceContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset SetupAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");

    [Theory]
    [InlineData("engulfing", TriggerFamily.Reversal)]
    [InlineData("stars", TriggerFamily.Reversal)]
    [InlineData("tweezers", TriggerFamily.Reversal)]
    [InlineData("fakeOut", TriggerFamily.Reversal)]
    [InlineData("perfectPullback", TriggerFamily.Continuation)]
    [InlineData("longPullback", TriggerFamily.Continuation)]
    [InlineData("shortPullback", TriggerFamily.Continuation)]
    public void EachAlternative_RoundTripsItsSpecificEvidenceAsOneVariant(string variant, TriggerFamily family)
    {
        var trigger = CreateTrigger(variant, family);

        var json = JsonSerializer.Serialize(trigger, JsonOptions);
        var restored = JsonSerializer.Deserialize<TriggerEvidenceDto>(json, JsonOptions)!;

        Assert.Equal(family, restored.Family.Value);
        Assert.IsType(trigger.VariantEvidence!.GetType(), restored.VariantEvidence);
        Assert.Equal(json, JsonSerializer.Serialize(restored, JsonOptions));
        using var document = JsonDocument.Parse(json);
        Assert.Equal(variant, document.RootElement.GetProperty("variantEvidence").GetProperty("variant").GetString());
        Assert.Equal(EvidenceAvailability.Unknown, restored.Concept.NormalizedId.Availability);
        Assert.Equal(trigger.Candles, restored.Candles);
    }

    [Fact]
    public void KnownCatalogId_DoesNotFillUnknownPatternClosureOrMovementExtreme()
    {
        var trigger = CreateTrigger("engulfing", TriggerFamily.Reversal) with
        {
            Concept = Concept("engulfing") with { NormalizedId = Known(123) },
            IsClosed = Unavailable<bool>(EvidenceAvailability.Unknown, "CLOSURE_NOT_VISIBLE"),
            MovementExtreme = UnknownAnchor("movement-tip"),
            VariantEvidence = null
        };

        var restored = RoundTrip(trigger);

        Assert.Equal(123, restored.Concept.NormalizedId.Value);
        Assert.Equal(EvidenceAvailability.Unknown, restored.IsClosed.Availability);
        Assert.Null(restored.IsClosed.Value);
        Assert.Null(restored.MovementExtreme.Price.Value);
        Assert.Null(restored.MovementExtreme.TimeUtc.Value);
        Assert.Null(restored.VariantEvidence);
    }

    [Fact]
    public void UnknownTrigger_RemainsUnidentifiedWithoutSelectingADefaultVariant()
    {
        var trigger = CreateTrigger("engulfing", TriggerFamily.Reversal) with
        {
            Concept = new CatalogReferenceDto(EvidenceAvailability.Unknown, null, null,
                Unavailable<int>(EvidenceAvailability.Unknown, "TRIGGER_NOT_IDENTIFIED"),
                null, null, null, "TRIGGER_NOT_IDENTIFIED"),
            Family = Unavailable<TriggerFamily>(EvidenceAvailability.Unknown, "FAMILY_NOT_IDENTIFIED"),
            VariantEvidence = null
        };

        var restored = RoundTrip(trigger);

        Assert.Empty(Validate(restored.Concept));
        Assert.Equal(EvidenceAvailability.Unknown, restored.Concept.Availability);
        Assert.Null(restored.Family.Value);
        Assert.Null(restored.VariantEvidence);
    }

    [Fact]
    public void Engulfing_PreservesCloseByIntentionWithoutClaimingIdealFullEnclosure()
    {
        TriggerVariantEvidenceDto variant = new EngulfingTriggerEvidenceDto(
            Reversal(), "left-range", Known(true), Known(false));

        var restored = Assert.IsType<EngulfingTriggerEvidenceDto>(RoundTrip(variant));

        Assert.True(restored.ClosesBeyondLeftRangeExtreme.Value);
        Assert.False(restored.EnclosesEntireLeftRange.Value);
    }

    [Fact]
    public void Stars_PreservesMultipleHeadCandlesAndDistinctRangeMidpoints()
    {
        TriggerVariantEvidenceDto variant = new StarsTriggerEvidenceDto(
            Reversal(), ["left-range"], ["head-1", "head-2"], ["right-shoulder"],
            Unavailable<bool>(EvidenceAvailability.Unknown, "INDECISION_NOT_DEFINED"), Known(true), Known(102.5m),
            Unavailable<bool>(EvidenceAvailability.Unknown, "MIDPOINT_EQUALITY_NOT_RESOLVED"), Known(false));

        var restored = Assert.IsType<StarsTriggerEvidenceDto>(RoundTrip(variant));

        Assert.Equal(new[] { "head-1", "head-2" }, restored.HeadReferences);
        Assert.NotEqual(restored.Reversal.RelevantRangeMidpoint.Value, restored.LeftShoulderMidpoint.Value);
        Assert.Equal(EvidenceAvailability.Unknown, restored.RightCloseReachesMidpoint.Availability);
        Assert.Equal("MIDPOINT_EQUALITY_NOT_RESOLVED", restored.RightCloseReachesMidpoint.ReasonCode);
        Assert.False(restored.RightCloseBeyondMidpoint.Value);
    }

    [Fact]
    public void Tweezers_UndefinedSimilarityDoesNotAcquireADefaultTolerance()
    {
        var trigger = CreateTrigger("tweezers", TriggerFamily.Reversal);

        var restored = Assert.IsType<TweezersTriggerEvidenceDto>(RoundTrip(trigger).VariantEvidence);

        Assert.Equal(EvidenceAvailability.Unknown, restored.RangesAreSimilar.Availability);
        Assert.Equal(EvidenceAvailability.Unknown, restored.SimilarityTolerance.Availability);
        Assert.Null(restored.SimilarityTolerance.Value);
    }

    [Fact]
    public void FakeOut_RetainsKnownMissingBreakRatherThanInferringItFromRejection()
    {
        TriggerVariantEvidenceDto variant = new FakeOutTriggerEvidenceDto(
            Reversal(), Bounds(), null, Known(false), Known(true));

        var restored = Assert.IsType<FakeOutTriggerEvidenceDto>(RoundTrip(variant));

        Assert.False(restored.ReferenceWasBroken.Value);
        Assert.True(restored.BreakWasRejected.Value);
        Assert.Null(restored.BreakDefinitionCode);
    }

    [Fact]
    public void StageTwoReversal_CanMarkMovementTipRequirementNotApplicable()
    {
        TriggerVariantEvidenceDto variant = new EngulfingTriggerEvidenceDto(
            Reversal() with
            {
                ReachesMovementExtreme = Unavailable<bool>(EvidenceAvailability.NotApplicable, "STAGE_TWO_INTERIOR_REBOUND")
            },
            "left-range", Known(true), Known(false));

        var restored = Assert.IsType<EngulfingTriggerEvidenceDto>(RoundTrip(variant));

        Assert.Equal(EvidenceAvailability.NotApplicable, restored.Reversal.ReachesMovementExtreme.Availability);
        Assert.Null(restored.Reversal.ReachesMovementExtreme.Value);
    }

    [Theory]
    [InlineData("perfectPullback")]
    [InlineData("longPullback")]
    [InlineData("shortPullback")]
    public void Pullback_CompletedPhasesCanBeRepresentedWithoutCandleClosure(string variant)
    {
        var trigger = CreateTrigger(variant, TriggerFamily.Continuation) with
        {
            IsClosed = Known(false),
            Phases =
            [
                Phase(TriggerPhaseKind.Breakout, 1),
                Phase(TriggerPhaseKind.Retest, 2),
                Phase(TriggerPhaseKind.Continuation, 3)
            ]
        };

        var restored = RoundTrip(trigger);

        Assert.False(restored.IsClosed.Value);
        Assert.All(restored.Phases, phase => Assert.True(phase.IsComplete.Value));
        Assert.Equal(new[] { 1, 2, 3 }, restored.Phases.Select(phase => phase.SequenceIndex.Value!.Value));
    }

    [Fact]
    public void Pullback_OhlcAndCandleCountDoNotFillUnknownIntrabarSequence()
    {
        var trigger = CreateTrigger("perfectPullback", TriggerFamily.Continuation) with
        {
            Phases =
            [
                Phase(TriggerPhaseKind.Breakout, 1) with
                {
                    SequenceIndex = Unavailable<int>(EvidenceAvailability.Unknown, "INTRABAR_ORDER_NOT_KNOWN"),
                    IsComplete = Unavailable<bool>(EvidenceAvailability.Unknown, "INTRABAR_ORDER_NOT_KNOWN")
                }
            ]
        };

        var restored = RoundTrip(trigger);
        var variant = Assert.IsType<PerfectPullbackTriggerEvidenceDto>(restored.VariantEvidence);

        Assert.Equal(2, variant.Pullback.TriggerFrameCandleCount.Value);
        Assert.Equal(EvidenceAvailability.Unknown, variant.Pullback.HasOrderedSequence.Availability);
        Assert.Null(variant.Pullback.HasOrderedSequence.Value);
        Assert.Null(restored.Phases[0].SequenceIndex.Value);
        Assert.Null(restored.Phases[0].IsComplete.Value);
        Assert.Equal(trigger.Candles, restored.Candles);
    }

    [Fact]
    public void LongPullback_PreservesRetestMidpointTimeAndLaterConfirmation()
    {
        var reviewedAt = SetupAt.AddHours(2);
        TriggerVariantEvidenceDto variant = new LongPullbackTriggerEvidenceDto(
            Pullback(), Known(true), new PriceTimeAnchorDto("retest-midpoint",
                Known(100.5m, ValidationSource.UserConfirmation, reviewedAt),
                Known(SetupAt, ValidationSource.UserConfirmation, reviewedAt)),
            Unavailable<bool>(EvidenceAvailability.Unknown, "MIDPOINT_REFERENCE_NOT_RESOLVED"));

        var restored = Assert.IsType<LongPullbackTriggerEvidenceDto>(RoundTrip(variant));

        Assert.Equal(SetupAt, restored.RetestMidpoint.TimeUtc.Value);
        Assert.Equal(reviewedAt, restored.RetestMidpoint.TimeUtc.AvailableAtUtc);
        Assert.Equal(ValidationSource.UserConfirmation, restored.RetestMidpoint.Price.Source);
        Assert.Null(restored.MidpointRemainsOnFavorableSide.Value);
    }

    [Fact]
    public void ShortPullback_PreservesDistanceAndAmplitudeWithoutRequiringContact()
    {
        var trigger = CreateTrigger("shortPullback", TriggerFamily.Continuation);

        var restored = Assert.IsType<ShortPullbackTriggerEvidenceDto>(RoundTrip(trigger).VariantEvidence);

        Assert.True(restored.RetestRemainsBeforeReference.Value);
        Assert.Equal(0.5m, restored.DistanceToReference.Value);
        Assert.Equal(1m, restored.RetestAmplitude.Value);
    }

    [Fact]
    public void Phases_PreserveRecordedOrderAndRestartedSequenceIdentity()
    {
        var phases = new[]
        {
            Phase(TriggerPhaseKind.Continuation, 3),
            Phase(TriggerPhaseKind.Breakout, 1),
            Phase(TriggerPhaseKind.Retest, 2),
            Phase(TriggerPhaseKind.Breakout, 1) with { ReferenceCode = "new-breakout", SequenceReferenceCode = "sequence-2" }
        };

        var restored = RoundTrip(phases);

        Assert.Equal(new[] { 3, 1, 2, 1 }, restored.Select(phase => phase.SequenceIndex.Value!.Value));
        Assert.Equal("sequence-1", restored[0].SequenceReferenceCode);
        Assert.Equal("sequence-2", restored[3].SequenceReferenceCode);
        Assert.Equal(phases[0].ReferenceBounds, restored[0].ReferenceBounds);
    }

    [Theory]
    [InlineData(98, 99, 104, 106, 110)]
    [InlineData(102, 101, 94, 96, 90)]
    public void RiskPlan_PreservesTechnicalStopAndNearerObstacleSeparatelyFromTarget(
        int stop, int invalidation, int obstacleLow, int obstacleHigh, int target)
    {
        var plan = new RiskPlanEvidenceDto(Anchor("entry", 100m), Anchor("sl", stop), Anchor("invalidation", invalidation),
            ["zone", "retest.extreme"], Known(1m), Obstacle(obstacleLow, obstacleHigh), Anchor("target", target));

        var restored = RoundTrip(plan);

        Assert.Empty(Validate(restored.FirstObstacle));
        Assert.Equal(100m, restored.Entry.Price.Value);
        Assert.Equal((decimal)stop, restored.StopLoss.Price.Value);
        Assert.Equal((decimal)invalidation, restored.TechnicalInvalidation.Price.Value);
        Assert.Equal(1m, restored.TechnicalMargin.Value);
        Assert.Equal(plan.InvalidationEvidenceReferences, restored.InvalidationEvidenceReferences);
        Assert.Equal((decimal)obstacleLow, restored.FirstObstacle.Reference!.InfluenceBounds.LowerPrice.Value);
        Assert.Equal((decimal)obstacleHigh, restored.FirstObstacle.Reference.InfluenceBounds.UpperPrice.Value);
        Assert.Equal((decimal)target, restored.Target.Price.Value);
        Assert.NotEqual(restored.FirstObstacle.Reference.InfluenceBounds.UpperPrice.Value, restored.Target.Price.Value);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown)]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void ObstacleAssessment_RetainsAvailabilityReasonAndReviewTime(EvidenceAvailability availability)
    {
        var reviewedAt = SetupAt.AddHours(2);
        var obstacle = new RiskObstacleEvidenceDto(availability, null,
            ValidationSource.UserConfirmation, SetupAt, reviewedAt, "EXPLICIT_MAP_ASSESSMENT");

        var restored = RoundTrip(obstacle);

        Assert.Empty(Validate(restored));
        Assert.Equal(obstacle, restored);
        Assert.Null(restored.Reference);
        Assert.Equal(reviewedAt, restored.AvailableAtUtc);
    }

    [Fact]
    public void RiskPlan_MissingPricesAndMarginRemainUnknown()
    {
        var plan = new RiskPlanEvidenceDto(UnknownAnchor("entry"), UnknownAnchor("sl"), UnknownAnchor("invalidation"), [],
            Unavailable<decimal>(EvidenceAvailability.Unknown, "MARGIN_NOT_DEFINED"),
            new RiskObstacleEvidenceDto(EvidenceAvailability.Unknown, null, null, null, null, "MAP_NOT_REVIEWED"),
            UnknownAnchor("target"));

        var restored = RoundTrip(plan);

        Assert.Null(restored.Entry.Price.Value);
        Assert.Null(restored.StopLoss.Price.Value);
        Assert.Null(restored.TechnicalInvalidation.Price.Value);
        Assert.Null(restored.TechnicalMargin.Value);
        Assert.Null(restored.Target.Price.Value);
        Assert.Equal(EvidenceAvailability.Unknown, restored.FirstObstacle.Availability);
    }

    [Fact]
    public void ObstacleContract_RejectsMissingPresentReferenceAndInconsistentAbsence()
    {
        var present = new RiskObstacleEvidenceDto(EvidenceAvailability.Present, null, null, null, null, null);
        var absent = Obstacle(104, 106) with { Availability = EvidenceAvailability.Absent, ReasonCode = null };

        Assert.Contains(Validate(present), error => error.MemberNames.Contains("Reference"));
        Assert.Contains(Validate(absent), error => error.MemberNames.Contains("Reference"));
        Assert.Contains(Validate(absent), error => error.MemberNames.Contains("ReasonCode"));
    }

    private static TriggerEvidenceDto CreateTrigger(string variant, TriggerFamily family) => new(
        "trigger", "zone", Concept(variant), Known(family),
        [new TriggerCandleEvidenceDto("left-range", "leftRange", Known(100m), Known(103m), Known(99m), Known(102m),
            Known(SetupAt.AddMinutes(-2)), Unavailable<DateTimeOffset>(EvidenceAvailability.Unknown, "CLOSE_TIME_NOT_KNOWN"),
            Known(false))],
        Known(false), Anchor("movement-tip", 99m), [], Variant(variant));

    private static TriggerVariantEvidenceDto Variant(string variant) => variant switch
    {
        "engulfing" => new EngulfingTriggerEvidenceDto(Reversal(), "left-range", Known(true), Known(false)),
        "stars" => new StarsTriggerEvidenceDto(Reversal(), ["left-range"], ["head"], ["right-shoulder"],
            Unavailable<bool>(EvidenceAvailability.Unknown, "INDECISION_NOT_DEFINED"), Known(true), Known(101m),
            Known(true), Known(false)),
        "tweezers" => new TweezersTriggerEvidenceDto(Reversal(), ["left-range", "right-range"],
            Unavailable<bool>(EvidenceAvailability.Unknown, "SIMILARITY_NOT_DEFINED"), Known(true), Known(true),
            Unavailable<decimal>(EvidenceAvailability.Unknown, "TOLERANCE_NOT_DEFINED")),
        "fakeOut" => new FakeOutTriggerEvidenceDto(Reversal(), Bounds(), "FULL_BAND", Known(true), Known(true)),
        "perfectPullback" => new PerfectPullbackTriggerEvidenceDto(Pullback(), Known(true),
            Unavailable<decimal>(EvidenceAvailability.Unknown, "TOLERANCE_NOT_DEFINED")),
        "longPullback" => new LongPullbackTriggerEvidenceDto(Pullback(), Known(true), Anchor("retest-midpoint", 100.5m), Known(true)),
        "shortPullback" => new ShortPullbackTriggerEvidenceDto(Pullback(), Known(true), Known(0.5m), Known(1m)),
        _ => throw new ArgumentOutOfRangeException(nameof(variant))
    };

    private static ReversalRequirementsDto Reversal() => new(Known(true), Known(true), ["left-range"], Known(100m),
        Unavailable<bool>(EvidenceAvailability.Unknown, "REJECTION_HALF_CRITERION_NOT_RESOLVED"));

    private static PullbackRequirementsDto Pullback() => new(
        Unavailable<bool>(EvidenceAvailability.Unknown, "INTRABAR_ORDER_NOT_KNOWN"), Known(true), Known(2));

    private static TriggerPhaseEvidenceDto Phase(TriggerPhaseKind kind, int index) => new(
        kind.ToString(), "sequence-1", Known(kind), Known(index), "zone", Bounds(),
        Anchor($"{kind}.start", 100m), Anchor($"{kind}.end", 101m), Anchor($"{kind}.extreme", 102m),
        ["left-range"], Known(true));

    private static RiskObstacleEvidenceDto Obstacle(decimal lower, decimal upper) => new(
        EvidenceAvailability.Present,
        new StructuralReferenceDto("first-checkpoint", Known(MapReferenceKind.Checkpoint),
            new CatalogReferenceDto(EvidenceAvailability.NotApplicable, null, null,
                Unavailable<int>(EvidenceAvailability.NotApplicable, "CHECKPOINT_NOT_FIGURE"),
                null, null, null, "CHECKPOINT_NOT_FIGURE"),
            Known(StructureDirection.Neutral), Known(60), Known(1),
            Known(lower > 100 ? LocationType.Resistance : LocationType.Support),
            new ZoneInfluenceBoundsDto(Known(lower), Known(upper), Known(SetupAt)), [], Known(true)),
        ValidationSource.UserInput, SetupAt, SetupAt, null);

    private static CatalogReferenceDto Concept(string code) => new(EvidenceAvailability.Present, code, code,
        Unavailable<int>(EvidenceAvailability.Unknown, "CATALOG_NOT_NORMALIZED"), ValidationSource.AiVision,
        SetupAt, SetupAt, null);

    private static ZoneInfluenceBoundsDto Bounds() => new(Known(99.5m), Known(100.5m), Known(SetupAt));

    private static PriceTimeAnchorDto Anchor(string code, decimal price) => new(code, Known(price), Known(SetupAt));

    private static PriceTimeAnchorDto UnknownAnchor(string code) => new(code,
        Unavailable<decimal>(EvidenceAvailability.Unknown, "PRICE_NOT_KNOWN"),
        Unavailable<DateTimeOffset>(EvidenceAvailability.Unknown, "TIME_NOT_KNOWN"));

    private static EvidenceValueDto<T> Known<T>(T value, ValidationSource source = ValidationSource.UserInput,
        DateTimeOffset? availableAt = null) where T : struct =>
        new(EvidenceAvailability.Present, value, source, null, availableAt, null, null);

    private static EvidenceValueDto<T> Unavailable<T>(EvidenceAvailability availability, string reason) where T : struct =>
        new(availability, null, null, null, null, null, reason);

    private static T RoundTrip<T>(T value) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;

    private static IReadOnlyList<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
