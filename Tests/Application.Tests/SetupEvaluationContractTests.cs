using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Application.DTOs.AiValidation.V2;
using Domain.Enums;

namespace Application.Tests;

public class SetupEvaluationContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset SetupAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");

    [Fact]
    public void Evaluation_DefaultsToThreePendingConditionsWithoutInventingSourceVersionsOrRevision()
    {
        var evaluation = Evaluation();

        var restored = RoundTrip(evaluation);

        Assert.Empty(Validate(restored));
        Assert.Equal(SetupConditionKind.Zone, restored.Zone.Condition);
        Assert.Equal(SetupConditionKind.Trigger, restored.Trigger.Condition);
        Assert.Equal(SetupConditionKind.Ratio, restored.Ratio.Condition);
        Assert.Equal(SetupConditionState.Pending, restored.Zone.State);
        Assert.Equal(SetupConditionState.Pending, restored.Trigger.State);
        Assert.Equal(SetupConditionState.Pending, restored.Ratio.State);
        Assert.Equal(SetupVerdict.Pending, restored.Verdict);
        Assert.Null(restored.EvaluationRevision);
        Assert.Null(restored.EvaluatedAtUtc);
        Assert.Equal(new SetupEvaluationVersionsDto(), restored.Versions);
    }

    [Theory]
    [InlineData(SetupConditionState.Passed, SetupConditionState.Passed, SetupConditionState.Passed, SetupVerdict.Valid)]
    [InlineData(SetupConditionState.Pending, SetupConditionState.Passed, SetupConditionState.Passed, SetupVerdict.Pending)]
    [InlineData(SetupConditionState.Passed, SetupConditionState.Pending, SetupConditionState.Passed, SetupVerdict.Pending)]
    [InlineData(SetupConditionState.Passed, SetupConditionState.Passed, SetupConditionState.Pending, SetupVerdict.Pending)]
    [InlineData(SetupConditionState.Pending, SetupConditionState.Pending, SetupConditionState.Pending, SetupVerdict.Pending)]
    [InlineData(SetupConditionState.Failed, SetupConditionState.Passed, SetupConditionState.Passed, SetupVerdict.Invalid)]
    [InlineData(SetupConditionState.Passed, SetupConditionState.Failed, SetupConditionState.Passed, SetupVerdict.Invalid)]
    [InlineData(SetupConditionState.Passed, SetupConditionState.Passed, SetupConditionState.Failed, SetupVerdict.Invalid)]
    [InlineData(SetupConditionState.Failed, SetupConditionState.Pending, SetupConditionState.Pending, SetupVerdict.Invalid)]
    [InlineData(SetupConditionState.Pending, SetupConditionState.Failed, SetupConditionState.Pending, SetupVerdict.Invalid)]
    [InlineData(SetupConditionState.Pending, SetupConditionState.Pending, SetupConditionState.Failed, SetupVerdict.Invalid)]
    [InlineData(SetupConditionState.Failed, SetupConditionState.Failed, SetupConditionState.Failed, SetupVerdict.Invalid)]
    public void Verdict_ComposesOnlyTheThreeSuppliedConditions(
        SetupConditionState zone, SetupConditionState trigger, SetupConditionState ratio, SetupVerdict expected)
    {
        var evaluation = Evaluation() with
        {
            Zone = new(SetupConditionKind.Zone) { State = zone },
            Trigger = new(SetupConditionKind.Trigger) { State = trigger },
            Ratio = new(SetupConditionKind.Ratio) { State = ratio }
        };

        var restored = RoundTrip(evaluation);

        Assert.Empty(Validate(restored));
        Assert.Equal(expected, evaluation.Verdict);
        Assert.Equal(expected, restored.Verdict);
    }

    [Fact]
    public void Deserialization_DoesNotTrustAStaleSerializedVerdict()
    {
        var document = JsonSerializer.SerializeToNode(Evaluation(), JsonOptions)!;
        document["verdict"] = (int)SetupVerdict.Valid;

        var restored = document.Deserialize<SetupEvaluationResultDto>(JsonOptions)!;

        Assert.Equal(SetupVerdict.Pending, restored.Verdict);
    }

    [Fact]
    public void NewRevision_RecomposesVerdictWithoutChangingOriginalConditionsOrVersions()
    {
        var original = Evaluation() with { EvaluationRevision = 1, Versions = new(StrategyVersion: "original-strategy") };
        var revised = original with
        {
            EvaluationRevision = 2,
            Versions = new(StrategyVersion: "reviewed-strategy"),
            Zone = original.Zone with { State = SetupConditionState.Failed }
        };

        Assert.Equal(SetupVerdict.Pending, original.Verdict);
        Assert.Equal(SetupConditionState.Pending, original.Zone.State);
        Assert.Equal("original-strategy", original.Versions.StrategyVersion);
        Assert.Equal(SetupVerdict.Invalid, revised.Verdict);
        Assert.Equal("reviewed-strategy", revised.Versions.StrategyVersion);
    }

    [Fact]
    public void Evaluation_RejectsMislabeledConditionsWithoutClaimingValidity()
    {
        var evaluation = Evaluation() with
        {
            Zone = new(SetupConditionKind.Trigger) { State = SetupConditionState.Passed },
            Trigger = new(SetupConditionKind.Trigger) { State = SetupConditionState.Passed },
            Ratio = new(SetupConditionKind.Ratio) { State = SetupConditionState.Passed }
        };

        Assert.Equal(SetupVerdict.Pending, evaluation.Verdict);
        Assert.Contains(Validate(evaluation), error => error.MemberNames.Contains("Zone"));
    }

    [Fact]
    public void MainCondition_CannotUseTheInternalNotApplicableState()
    {
        var evaluation = Evaluation() with
        {
            Zone = new(SetupConditionKind.Zone) { State = (SetupConditionState)3 }
        };

        Assert.Equal(SetupVerdict.Pending, evaluation.Verdict);
        Assert.Contains(Validate(evaluation), error => error.MemberNames.Contains("Zone.State"));
    }

    [Theory]
    [InlineData(SetupRequirementKind.Mandatory)]
    [InlineData(SetupRequirementKind.QualityCriterion)]
    [InlineData(SetupRequirementKind.ProfileFilter)]
    public void Requirement_RetainsItsRoleNormativeBasisReasonsAndEvidence(SetupRequirementKind kind)
    {
        var requirement = new SetupRequirementResultDto("REVIEWED_CRITERION", kind)
        {
            State = SetupRequirementState.NotApplicable,
            NormativeReferences = ["CHECKLIST_TRADING:C1:Z3"],
            ReasonCodes = ["STAGE_TWO_NO_1234"],
            EvidenceReferences = ["evidence-1.zone.originStageId"]
        };

        var restored = RoundTrip(requirement);

        Assert.Empty(Validate(restored));
        Assert.Equal(kind, restored.Kind);
        Assert.Equal(SetupRequirementState.NotApplicable, restored.State);
        Assert.Equal(requirement.NormativeReferences, restored.NormativeReferences);
        Assert.Equal(requirement.ReasonCodes, restored.ReasonCodes);
        Assert.Equal(requirement.EvidenceReferences, restored.EvidenceReferences);
    }

    [Fact]
    public void UnjustifiedInapplicability_IsReportedThroughTheEvaluationEvidencePath()
    {
        var evaluation = Evaluation() with
        {
            Zone = new(SetupConditionKind.Zone)
            {
                Requirements =
                [
                    new("STRIKES", SetupRequirementKind.Mandatory) { State = SetupRequirementState.NotApplicable }
                ]
            }
        };

        Assert.Contains(Validate(evaluation), error => error.MemberNames.Contains("Zone.Requirements[0].ReasonCodes"));
        Assert.Equal(SetupVerdict.Pending, evaluation.Verdict);
    }

    [Fact]
    public void Result_RoundTripsAllVersionDimensionsAndEvidenceLinkage()
    {
        var result = Evaluation() with
        {
            EvaluationRevision = 2,
            EvaluatedAtUtc = SetupAt.AddHours(2),
            Versions = new("strategy-1", "evidence-2", "graphical-3", "classification-4", "score-5", "metrics-6", "prompt-7", "extraction-8"),
            Trigger = new(SetupConditionKind.Trigger)
            {
                State = SetupConditionState.Pending,
                NormativeReferences = ["CHECKLIST_TRADING:C2:G4"],
                ReasonCodes = ["INTRABAR_ORDER_NOT_KNOWN"],
                EvidenceReferences = ["evidence-1.trigger.phases"]
            }
        };

        var restored = RoundTrip(result);

        Assert.Equal("evaluation-1", restored.EvaluationReferenceCode);
        Assert.Equal("evidence-1", restored.EvidenceReferenceCode);
        Assert.Equal(2, restored.EvaluationRevision);
        Assert.Equal(result.Versions, restored.Versions);
        Assert.Equal(result.EvaluatedAtUtc, restored.EvaluatedAtUtc);
        Assert.Equal(result.Trigger.NormativeReferences, restored.Trigger.NormativeReferences);
        Assert.Equal(result.Trigger.ReasonCodes, restored.Trigger.ReasonCodes);
        Assert.Equal(result.Trigger.EvidenceReferences, restored.Trigger.EvidenceReferences);
    }

    [Fact]
    public void RealTradingReview_DefaultsToPendingIndependentlyOfGraphicalValidity()
    {
        var result = Evaluation() with
        {
            Zone = new(SetupConditionKind.Zone) { State = SetupConditionState.Passed },
            Trigger = new(SetupConditionKind.Trigger) { State = SetupConditionState.Passed },
            Ratio = new(SetupConditionKind.Ratio) { State = SetupConditionState.Passed }
        };
        var review = new RealTradingReviewDto(result.EvaluationReferenceCode);

        Assert.Equal(SetupVerdict.Valid, result.Verdict);
        Assert.Empty(Validate(review));
        Assert.Equal(RealTradingReviewState.Pending, review.State);
        Assert.Null(review.EvaluationRevision);
        Assert.Null(review.ReviewerUserId);
        Assert.Null(review.ReviewedAtUtc);
        Assert.Null(review.Rationale);
    }

    [Theory]
    [InlineData(RealTradingReviewState.Enabled)]
    [InlineData(RealTradingReviewState.NotEnabled)]
    public void ManualDecision_RequiresKnownRevisionReviewerDateAndRationale(RealTradingReviewState state)
    {
        var review = new RealTradingReviewDto("evaluation-1", State: state);

        var errors = Validate(review);

        Assert.Contains(errors, error => error.MemberNames.Contains("EvaluationRevision"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ReviewerUserId"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ReviewedAtUtc"));
        Assert.Contains(errors, error => error.MemberNames.Contains("Rationale"));
    }

    [Theory]
    [InlineData(RealTradingReviewState.Enabled)]
    [InlineData(RealTradingReviewState.NotEnabled)]
    public void ManualDecision_RoundTripsItsExactEvaluationRevisionAndFoundation(RealTradingReviewState state)
    {
        var review = new RealTradingReviewDto("evaluation-1", 2, state, "reviewer-1", SetupAt.AddHours(2),
            "The reviewer considered the personal trading record and the selected operating profile.");

        var restored = RoundTrip(review);

        Assert.Empty(Validate(restored));
        Assert.Equal(review, restored);
        Assert.Equal(2, restored.EvaluationRevision);
    }

    [Fact]
    public void ManualReview_RejectsUnknownStateMissingEvaluationAndNonPositiveRevision()
    {
        var review = new RealTradingReviewDto(" ", 0);

        var errors = Validate(review);

        Assert.Contains(errors, error => error.MemberNames.Contains("EvaluationReferenceCode"));
        Assert.Contains(errors, error => error.MemberNames.Contains("EvaluationRevision"));
        Assert.Contains(Validate(review with { State = (RealTradingReviewState)99 }), error => error.MemberNames.Contains("State"));
    }

    [Fact]
    public void FullEvidenceDocument_RoundTripsUnknownObservationsWithoutInventingFacts()
    {
        var evidence = UnknownEvidence();

        var json = JsonSerializer.Serialize(evidence, JsonOptions);
        var restored = JsonSerializer.Deserialize<TradeSetupEvidenceDto>(json, JsonOptions)!;

        Assert.Equal(json, JsonSerializer.Serialize(restored, JsonOptions));
        Assert.Equal("strategy-1", restored.StrategyVersion);
        Assert.Equal("evidence-2", restored.EvidenceSchemaVersion);
        Assert.Equal(SetupAt.AddHours(2), restored.EvaluatedAtUtc);
        Assert.Null(restored.Context.SetupAtUtc.Value);
        Assert.Null(restored.Context.SetupAtUtc.AvailableAtUtc);
        Assert.Null(restored.Trigger.VariantEvidence);
        Assert.Null(restored.RiskPlan.TechnicalMargin.Value);
        Assert.Equal(EvidenceAvailability.Unknown, restored.ContextualEvidence.MainFigure.Availability);
        Assert.Equal(evidence.Conflicts[0].ObservationReferences, restored.Conflicts[0].ObservationReferences);
        Assert.False(restored.Conflicts[0].IsResolved);
    }

    [Fact]
    public void EmptyContextCollections_DoNotBecomeACompletedReviewBySerialization()
    {
        var context = UnknownEvidence().ContextualEvidence;
        var reviewed = context with
        {
            StructuralReferencesComplete = Known(true),
            ConvergencesComplete = Known(true),
            FactorsComplete = Known(true)
        };

        var unknownRestored = RoundTrip(context);
        var reviewedRestored = RoundTrip(reviewed);

        Assert.Empty(unknownRestored.StructuralReferences);
        Assert.Empty(unknownRestored.Convergences);
        Assert.Empty(unknownRestored.Factors);
        Assert.Null(unknownRestored.StructuralReferencesComplete.Value);
        Assert.Null(unknownRestored.ConvergencesComplete.Value);
        Assert.Null(unknownRestored.FactorsComplete.Value);
        Assert.True(reviewedRestored.StructuralReferencesComplete.Value);
        Assert.True(reviewedRestored.ConvergencesComplete.Value);
        Assert.True(reviewedRestored.FactorsComplete.Value);
    }

    [Fact]
    public void Context_PreservesPrimaryFigureCrossFrameConvergenceAndOpposingFactorsSeparately()
    {
        var context = UnknownEvidence().ContextualEvidence with
        {
            MainFigure = new CatalogReferenceDto(EvidenceAvailability.Present, "CHANNEL", "Canal",
                Unknown<int>(), ValidationSource.UserInput, SetupAt, SetupAt, null),
            ClassificationStructureReferenceCode = "structure-1",
            StructuralReferences =
            [
                Structure("structure-1", 60, StructureDirection.Upward),
                Structure("structure-2", 240, StructureDirection.Downward)
            ],
            Convergences =
            [
                new("convergence-1", ["structure-1", "structure-2"], Known(true),
                    ["zone.influenceBounds"], "The reviewer identified both references at the working zone.")
            ],
            Factors =
            [
                new("factor-rsi", "RSI", Known(ContextualFactorEffect.Favorable), Known(63m), Known(60),
                    ["structure-1"], "A favorable RSI observation was supplied."),
                new("factor-structure", "OPPOSING_STRUCTURE", Known(ContextualFactorEffect.Contrary),
                    new(EvidenceAvailability.NotApplicable, null, null, null, null, null, "QUALITATIVE_FACTOR"),
                    Known(240), ["structure-2"], "An opposing higher-frame structure was supplied.")
            ]
        };

        var restored = RoundTrip(context);

        Assert.Equal(context.MainFigure, restored.MainFigure);
        Assert.Equal("structure-1", restored.ClassificationStructureReferenceCode);
        Assert.Equal(new int?[] { 60, 240 }, restored.StructuralReferences.Select(reference => reference.FrameId.Value));
        Assert.Equal(context.Convergences[0].StructuralReferenceCodes, restored.Convergences[0].StructuralReferenceCodes);
        Assert.True(restored.Convergences[0].IsPresent.Value);
        Assert.Equal(ContextualFactorEffect.Favorable, restored.Factors[0].Effect.Value);
        Assert.Equal(ContextualFactorEffect.Contrary, restored.Factors[1].Effect.Value);
        Assert.Equal(63m, restored.Factors[0].ObservedValue.Value);
        Assert.Equal(EvidenceAvailability.NotApplicable, restored.Factors[1].ObservedValue.Availability);
        Assert.Null(restored.FactorsComplete.Value);
    }

    private static SetupEvaluationResultDto Evaluation() => new("evaluation-1", "evidence-1", new());

    private static TradeSetupEvidenceDto UnknownEvidence() => new(
        "strategy-1", "evidence-2", SetupAt.AddHours(2),
        new EvaluationContextDto(10, 20, Unknown<int>(), Unknown<int>(), Unknown<DateTimeOffset>(), Unknown<EvaluationTimingMode>()),
        new ZoneEvidenceDto("zone", Unknown<MapReferenceKind>(), UnknownConcept(), Bounds(), [], Unknown<int>(), Unknown<int>(),
            Unknown<DateTimeOffset>(), Unknown<DateTimeOffset>(), Unknown<bool>(), Unknown<bool>(), Unknown<int>(), Unknown<DateTimeOffset>()),
        new TriggerEvidenceDto("trigger", "zone", UnknownConcept(), Unknown<TriggerFamily>(), [], Unknown<bool>(), Anchor("tip"), [], null),
        new RiskPlanEvidenceDto(Anchor("entry"), Anchor("sl"), Anchor("invalidation"), [], Unknown<decimal>(),
            new RiskObstacleEvidenceDto(EvidenceAvailability.Unknown, null, null, null, null, "MAP_NOT_REVIEWED"), Anchor("target")),
        new ContextualEvidenceDto(UnknownConcept(), null, [], Unknown<bool>(), [], Unknown<bool>(), [], Unknown<bool>()),
        [new EvidenceConflictDto("trigger.isClosed", ["provider.trigger.isClosed", "human.trigger.isClosed"])]);

    private static StructuralReferenceDto Structure(string code, int frame, StructureDirection direction) => new(
        code, Known(MapReferenceKind.WorkingLine), UnknownConcept(), Known(direction), Known(frame), Known(1),
        Unknown<LocationType>(), Bounds(), [], Known(true));

    private static CatalogReferenceDto UnknownConcept() => new(EvidenceAvailability.Unknown, null, null, Unknown<int>(),
        null, null, null, "CONCEPT_NOT_IDENTIFIED");

    private static ZoneInfluenceBoundsDto Bounds() => new(Unknown<decimal>(), Unknown<decimal>(), Unknown<DateTimeOffset>());

    private static PriceTimeAnchorDto Anchor(string code) => new(code, Unknown<decimal>(), Unknown<DateTimeOffset>());

    private static EvidenceValueDto<T> Known<T>(T value) where T : struct =>
        new(EvidenceAvailability.Present, value, ValidationSource.UserConfirmation, SetupAt, SetupAt.AddHours(2), null, null);

    private static EvidenceValueDto<T> Unknown<T>() where T : struct =>
        new(EvidenceAvailability.Unknown, null, null, null, null, null, "NOT_OBSERVED");

    private static T RoundTrip<T>(T value) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;

    private static IReadOnlyList<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
