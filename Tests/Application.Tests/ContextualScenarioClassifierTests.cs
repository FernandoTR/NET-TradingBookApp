using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Common;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class ContextualScenarioClassifierTests
{
    private static readonly DateTimeOffset ObservedAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");
    private readonly ContextualScenarioClassifier _classifier = new();

    [Theory]
    [InlineData(ContextualBaseContext.Favorable, false, false, true, GradeType.A)]
    [InlineData(ContextualBaseContext.Favorable, false, true, true, GradeType.AB)]
    [InlineData(ContextualBaseContext.Favorable, false, false, false, GradeType.AB)]
    [InlineData(ContextualBaseContext.Neutral, true, false, false, GradeType.BA)]
    [InlineData(ContextualBaseContext.Neutral, false, false, false, GradeType.B)]
    [InlineData(ContextualBaseContext.Neutral, false, true, false, GradeType.BC)]
    [InlineData(ContextualBaseContext.Contrary, true, false, false, GradeType.CB)]
    [InlineData(ContextualBaseContext.Contrary, false, true, false, GradeType.C)]
    [InlineData(ContextualBaseContext.Neutral, true, true, false, null)]
    public void ApprovedContextTable_ProducesTheAgreedClassification(
        ContextualBaseContext context, bool favorable, bool contrary, bool convergence, GradeType? expected)
    {
        var input = Input(context, favorable, contrary, convergence);

        var result = _classifier.Evaluate(input);

        Assert.Equal(expected, result.Grade);
        Assert.Equal(expected.HasValue ? ContextualScoreState.Complete : ContextualScoreState.Partial, result.State);
        Assert.Equal(context, result.BaseContext.Value);
        Assert.NotNull(result.RuleCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Justification));
        Assert.Equal(ContextualScoreDefinitions.V1.ClassificationRulesVersion, result.ClassificationRulesVersion);
        Assert.All(result.Components.Where(component => component.Availability == EvidenceAvailability.NotApplicable),
            component => Assert.False(string.IsNullOrWhiteSpace(component.ReasonCode)));
    }

    [Theory]
    [InlineData(ContextualFactorEffect.Favorable, GradeType.BA)]
    [InlineData(ContextualFactorEffect.Contrary, GradeType.BC)]
    [InlineData(ContextualFactorEffect.Neutral, GradeType.B)]
    public void ApprovedManualPredominance_ProducesCalculatedGradeAndRetainsBothArgumentSnapshots(
        ContextualFactorEffect predominance, GradeType expected)
    {
        var original = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        var input = original with { PredominanceResolution = Resolution(original, predominance) };
        var jsonBefore = JsonSerializer.Serialize(input);

        var result = _classifier.Evaluate(RoundTrip(input));

        Assert.Equal(ContextualScoreState.Complete, result.State);
        Assert.Equal(expected, result.Grade);
        Assert.Equal(ContextualFactorEffect.Favorable, result.Factors[0].Effect.Value);
        Assert.Equal(ContextualFactorEffect.Contrary, result.Factors[1].Effect.Value);
        Assert.Single(result.PredominanceResolution!.FavorableArguments);
        Assert.Single(result.PredominanceResolution.ContraryArguments);
        Assert.Equal("reviewer-1", result.PredominanceResolution.ReviewerUserId);
        Assert.Equal(ObservedAt.AddHours(2), result.PredominanceResolution.ResolvedAtUtc);
        Assert.Contains("CONTEXT_PREDOMINANCE_RESOLVED", result.ReasonCodes);
        Assert.Equal(jsonBefore, JsonSerializer.Serialize(input));
        Assert.Equal(ContextualScoreState.Partial, _classifier.Evaluate(original).State);
    }

    [Theory]
    [InlineData(false, false, GradeType.B)]
    [InlineData(true, false, GradeType.BA)]
    [InlineData(false, true, GradeType.BC)]
    [InlineData(true, true, null)]
    public void PivotLine_StartsAtNeutralBaseWithoutNeedingASuppliedBaseAssessment(bool favorable, bool contrary, GradeType? expected)
    {
        var input = Input(null, favorable, contrary, pivot: true);

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualBaseContext.Neutral, result.BaseContext.Value);
        Assert.Equal(ValidationSource.DeterministicRule, result.BaseContext.Source);
        Assert.Equal("PIVOT_LINE_BASE_NEUTRAL", result.BaseContext.ReasonCode);
        Assert.Equal(expected, result.Grade);
    }

    [Fact]
    public void PivotLine_DoesNotAcceptAnOpposingBaseAssessmentAsAnOverride()
    {
        var result = _classifier.Evaluate(Input(ContextualBaseContext.Favorable, pivot: true));

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Equal(ContextualBaseContext.Neutral, result.BaseContext.Value);
        Assert.Null(result.Grade);
        Assert.Contains("PIVOT_BASE_CONTEXT_CONFLICT", result.ReasonCodes);
    }

    [Fact]
    public void EmptyUnreviewedFactors_DoNotProveAbsenceOfModifiers()
    {
        var input = Input(ContextualBaseContext.Neutral);
        input = input with { Evidence = input.Evidence with { FactorsComplete = Unknown<bool>() } };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Equal(ContextualBaseContext.Neutral, result.BaseContext.Value);
        Assert.Null(result.Grade);
        Assert.Contains("FACTOR_REVIEW_INCOMPLETE", result.ReasonCodes);
        Assert.Contains(result.Components, component => component.Code == "HAS_FAVORABLE_FACTORS" &&
            component.Availability == EvidenceAvailability.Unknown);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void ExplicitlyUnavailableFactors_AreNotInventedAsModifiers(EvidenceAvailability availability)
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true);
        input = input with
        {
            Evidence = input.Evidence with
            {
                Factors = [input.Evidence.Factors[0] with
                {
                    Effect = new(availability, null, ValidationSource.UserConfirmation, ObservedAt, ObservedAt, null, "FACTOR_NOT_PRESENT_OR_APPLICABLE")
                }]
            }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Complete, result.State);
        Assert.Equal(GradeType.B, result.Grade);
        Assert.Equal(availability, result.Factors[0].Effect.Availability);
    }

    [Fact]
    public void UnknownFactorEffect_DoesNotBecomeAConfirmedAbsence()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true);
        input = input with { Evidence = input.Evidence with { Factors = [input.Evidence.Factors[0] with { Effect = Unknown<ContextualFactorEffect>() }] } };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("FACTOR_EVIDENCE_UNKNOWN", result.ReasonCodes);
    }

    [Fact]
    public void UnsourcedFactorArgument_DoesNotQualifyAsAnEvidencedModifier()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true);
        input = input with { Evidence = input.Evidence with { Factors = [input.Evidence.Factors[0] with { EvidenceReferences = [] }] } };

        var result = _classifier.Evaluate(input);

        Assert.Null(result.Grade);
        Assert.Contains("FACTOR_EVIDENCE_UNKNOWN", result.ReasonCodes);
    }

    [Fact]
    public void RuleSpecificRequirements_DoNotDemandIrrelevantConvergenceOrFavorableModifiers()
    {
        var input = Input(ContextualBaseContext.Favorable, contrary: true);
        input = input with
        {
            HasSufficientConvergence = Unknown<bool>(),
            Evidence = input.Evidence with
            {
                FactorsComplete = Unknown<bool>(),
                ConvergencesComplete = Unknown<bool>(),
                Factors = [.. input.Evidence.Factors, Factor("unknown-factor", ContextualFactorEffect.Favorable) with { Effect = Unknown<ContextualFactorEffect>() }]
            }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Complete, result.State);
        Assert.Equal(GradeType.AB, result.Grade);
        Assert.Contains(result.Components, component => component.Code == "SUFFICIENT_CONVERGENCE" && component.Availability == EvidenceAvailability.NotApplicable);
    }

    [Fact]
    public void ContraryContextWithConfirmedFavorableFactor_DoesNotRequireProofOfAbsentContraryFactors()
    {
        var input = Input(ContextualBaseContext.Contrary, favorable: true);
        input = input with { Evidence = input.Evidence with { FactorsComplete = Unknown<bool>() } };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Complete, result.State);
        Assert.Equal(GradeType.CB, result.Grade);
    }

    [Fact]
    public void UnknownNecessaryConvergence_KeepsTheKnownFavorableContextPartial()
    {
        var input = Input(ContextualBaseContext.Favorable) with { HasSufficientConvergence = Unknown<bool>() };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Equal(ContextualBaseContext.Favorable, result.BaseContext.Value);
        Assert.Null(result.Grade);
        Assert.Contains("CONVERGENCE_ASSESSMENT_REQUIRED", result.ReasonCodes);
    }

    [Fact]
    public void SelectedFigureAlone_DoesNotCreditSufficientConvergence()
    {
        var input = Input(ContextualBaseContext.Favorable, convergence: true);
        input = input with { Evidence = input.Evidence with { Convergences = [] } };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("CONVERGENCE_EVIDENCE_MISSING", result.ReasonCodes);
    }

    [Fact]
    public void InsufficientConvergence_RequiresAReviewedCollectionRatherThanAnEmptyUnknownList()
    {
        var input = Input(ContextualBaseContext.Favorable);
        input = input with { Evidence = input.Evidence with { ConvergencesComplete = Unknown<bool>() } };

        var result = _classifier.Evaluate(input);

        Assert.Null(result.Grade);
        Assert.Contains("CONVERGENCE_REVIEW_INCOMPLETE", result.ReasonCodes);
    }

    [Fact]
    public void Convergence_UsesAKnownCompleteWitnessIndependentlyOfListOrder()
    {
        var input = Input(ContextualBaseContext.Favorable, convergence: true);
        var incompleteStructure = Structure("unknown-frame", input.Evidence.MainFigure) with { FrameId = Unknown<int>() };
        input = input with
        {
            Evidence = input.Evidence with
            {
                StructuralReferences = [.. input.Evidence.StructuralReferences, incompleteStructure],
                Convergences =
                [
                    new("incomplete", ["primary", "unknown-frame"], Known(true), ["zone"], "Incomplete frame evidence."),
                    .. input.Evidence.Convergences
                ]
            }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Complete, result.State);
        Assert.Equal(GradeType.A, result.Grade);
        Assert.Contains("higher-frame", result.EvidenceReferences);
    }

    [Fact]
    public void MissingConvergenceStructureFrame_PreventsACompleteClassification()
    {
        var input = Input(ContextualBaseContext.Favorable, convergence: true);
        input = input with
        {
            Evidence = input.Evidence with
            {
                StructuralReferences = [input.Evidence.StructuralReferences[0], input.Evidence.StructuralReferences[1] with { FrameId = Unknown<int>() }]
            }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("STRUCTURE_FRAME_REQUIRED", result.ReasonCodes);
    }

    [Fact]
    public void MissingBaseContext_IsNotInferredFromTheFigureOrItsStage()
    {
        var result = _classifier.Evaluate(Input(null));

        Assert.Equal(ContextualScoreState.NotEvaluable, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("BASE_CONTEXT_REQUIRED", result.ReasonCodes);
    }

    [Fact]
    public void AmbiguousFigure_RemainsNotEvaluableDespiteAnOtherwiseKnownContext()
    {
        var input = Input(ContextualBaseContext.Favorable);
        input = input with
        {
            MainFigure = input.MainFigure with
            {
                ResolvedReference = null,
                MatchedFigure = null,
                Concept = Unknown<FigureConceptKind>(),
                ReasonCodes = ["FIGURE_CATALOG_AMBIGUOUS"]
            }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.NotEvaluable, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("FIGURE_CATALOG_AMBIGUOUS", result.ReasonCodes);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void KnownStructureContext_DoesNotRequireAnInapplicableStageTwoFigure(EvidenceAvailability availability)
    {
        var input = Input(ContextualBaseContext.Neutral);
        var figure = new CatalogReferenceDto(availability, null, null,
            new(availability, null, null, null, null, null, "NO_STAGE_TWO_FIGURE"),
            ValidationSource.UserConfirmation, null, null, "NO_STAGE_TWO_FIGURE");
        input = input with
        {
            MainFigure = new("figure-semantics-v1", figure, null, null,
                new(availability, null, null, null, null, null, "NO_STAGE_TWO_FIGURE"), [], ["NO_STAGE_TWO_FIGURE"]),
            Evidence = input.Evidence with { MainFigure = figure }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Complete, result.State);
        Assert.Equal(GradeType.B, result.Grade);
    }

    [Fact]
    public void FigureResolution_MustCorrespondToTheSuppliedPrimaryFigure()
    {
        var input = Input(ContextualBaseContext.Favorable);
        input = input with { Evidence = input.Evidence with { MainFigure = Input(null, pivot: true).Evidence.MainFigure } };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.NotEvaluable, result.State);
        Assert.Contains("FIGURE_EVIDENCE_MISMATCH", result.ReasonCodes);
    }

    [Fact]
    public void MissingPrimaryStructureDirectionOrFrame_PreservesContextButWithholdsGrade()
    {
        var input = Input(ContextualBaseContext.Neutral);
        input = input with
        {
            Evidence = input.Evidence with
            {
                StructuralReferences = [input.Evidence.StructuralReferences[0] with { Direction = Unknown<StructureDirection>(), FrameId = Unknown<int>() }]
            }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Equal(ContextualBaseContext.Neutral, result.BaseContext.Value);
        Assert.Null(result.Grade);
        Assert.Contains("STRUCTURE_DIRECTION_REQUIRED", result.ReasonCodes);
        Assert.Contains("STRUCTURE_FRAME_REQUIRED", result.ReasonCodes);
    }

    [Fact]
    public void ManualResolution_RejectsMissingAttributionDateAndRationale()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        var resolution = Resolution(input, ContextualFactorEffect.Favorable) with
        {
            ReviewerUserId = " ", ResolvedAtUtc = default, Rationale = ""
        };
        var errors = new List<ValidationResult>();

        Assert.False(Validator.TryValidateObject(resolution, new ValidationContext(resolution), errors, true));
        Assert.Contains(errors, error => error.MemberNames.Contains("ReviewerUserId"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ResolvedAtUtc"));
        Assert.Contains(errors, error => error.MemberNames.Contains("Rationale"));
        var result = _classifier.Evaluate(input with { PredominanceResolution = resolution });
        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("CONTEXT_RESOLUTION_INVALID", result.ReasonCodes);
    }

    [Fact]
    public void ManualResolution_MustIncludeBothArgumentGroups()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        input = input with { PredominanceResolution = Resolution(input, ContextualFactorEffect.Favorable) with { ContraryArguments = [] } };

        var result = _classifier.Evaluate(input);

        Assert.Null(result.Grade);
        Assert.Contains("CONTEXT_RESOLUTION_INVALID", result.ReasonCodes);
        Assert.Equal(2, result.Factors.Count);
    }

    [Fact]
    public void ManualResolution_BecomesStaleWhenAFactChangesEvenIfItsReferenceCodeIsUnchanged()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        input = input with { PredominanceResolution = Resolution(input, ContextualFactorEffect.Favorable) };
        input = input with
        {
            Evidence = input.Evidence with { Factors = [input.Evidence.Factors[0] with { ObservedValue = Known(45m) }, input.Evidence.Factors[1]] }
        };

        var result = _classifier.Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, result.State);
        Assert.Null(result.Grade);
        Assert.Contains("CONTEXT_RESOLUTION_STALE", result.ReasonCodes);
        Assert.Equal(63m, result.PredominanceResolution!.FavorableArguments[0].ObservedValue.Value);
        Assert.Equal(45m, result.Factors[0].ObservedValue.Value);
    }

    [Fact]
    public void ManualResolution_BecomesStaleWhenANewOpposingArgumentAppears()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        input = input with { PredominanceResolution = Resolution(input, ContextualFactorEffect.Contrary) };
        input = input with { Evidence = input.Evidence with { Factors = [.. input.Evidence.Factors, Factor("new-contrary", ContextualFactorEffect.Contrary)] } };

        var result = _classifier.Evaluate(input);

        Assert.Null(result.Grade);
        Assert.Contains("CONTEXT_RESOLUTION_STALE", result.ReasonCodes);
        Assert.Equal(3, result.Factors.Count);
    }

    [Fact]
    public void ManualResolution_DoesNotClaimToResolveUnreviewedUnknownFactors()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        input = input with
        {
            PredominanceResolution = Resolution(input, ContextualFactorEffect.Neutral),
            Evidence = input.Evidence with { FactorsComplete = Known(false) }
        };

        var result = _classifier.Evaluate(input);

        Assert.Null(result.Grade);
        Assert.Contains("CONTEXT_RESOLUTION_REQUIRES_COMPLETE_FACTOR_REVIEW", result.ReasonCodes);
    }

    [Fact]
    public void Definition_RoundTripsRulesAndExplicitPositionsIndependentlyOfGradeEnumNumbers()
    {
        var definition = RoundTrip(_classifier.Definition);

        Assert.Equal("contextual-rules-v1", definition.ClassificationRulesVersion);
        Assert.Equal("contextual-ordinal-v1", definition.ScoreVersion);
        Assert.Equal(ContextualBaseContext.Neutral, definition.PivotLineBaseContext);
        Assert.Equal(new[] { GradeType.A, GradeType.AB, GradeType.BA, GradeType.B, GradeType.BC, GradeType.CB, GradeType.C },
            definition.Positions.Select(position => position.Grade));
        Assert.Equal(new[] { 6, 5, 4, 3, 2, 1, 0 }, definition.Positions.Select(position => position.ContextRank));
        Assert.Equal(0, (int)GradeType.A);
        Assert.Equal(6, (int)GradeType.C);
        Assert.All(definition.Rules, rule => Assert.False(string.IsNullOrWhiteSpace(rule.Justification)));
        Assert.All(definition.Rules, rule => Assert.NotEmpty(rule.NormativeReferences));
        Assert.Equal(definition.Rules.Count, definition.Rules.Select(rule => rule.Code).Distinct().Count());
    }

    [Fact]
    public void ApplicationRegistration_ProvidesTheVersionedClassifier()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddApplicationServices();
        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var classifier = scope.ServiceProvider.GetRequiredService<IContextualScenarioClassifier>();

        Assert.Equal(GradeType.B, classifier.Evaluate(Input(ContextualBaseContext.Neutral)).Grade);
    }

    private static ContextualScoreInputDto Input(ContextualBaseContext? context, bool favorable = false, bool contrary = false,
        bool convergence = false, bool pivot = false)
    {
        var reference = new CatalogReferenceDto(EvidenceAvailability.Present, pivot ? "LP" : "CA", pivot ? "LINEA PIVOTE" : "CANAL ASCENDENTE",
            Known(pivot ? 734 : 531), ValidationSource.UserConfirmation, ObservedAt, ObservedAt.AddMinutes(1), null);
        var figure = new FigureResolutionResultDto(ContextualFigureResolver.MappingVersion, reference, reference,
            new(pivot ? 734 : 531, reference.ConceptCode, reference.ObservedLabel, true),
            Known(pivot ? FigureConceptKind.PivotLine : FigureConceptKind.AscendingChannel), [], ["FIGURE_CATALOG_RESOLVED"]);
        var direction = context == ContextualBaseContext.Contrary ? StructureDirection.Downward :
            context is null or ContextualBaseContext.Neutral ? StructureDirection.Neutral : StructureDirection.Upward;
        var primary = Structure("primary", reference) with { Direction = Known(direction) };
        var higher = Structure("higher-frame", reference) with { FrameId = Known(240), Direction = Known(direction) };
        var factors = new List<ContextualFactorEvidenceDto>();
        if (favorable) factors.Add(Factor("favorable", ContextualFactorEffect.Favorable));
        if (contrary) factors.Add(Factor("contrary", ContextualFactorEffect.Contrary));
        var evidence = new ContextualEvidenceDto(reference, "primary", [primary, higher], Known(true),
            convergence ? [new("convergence", ["primary", "higher-frame"], Known(true), ["zone"], "Explicit favorable convergence assessment.")] : [],
            Known(true), factors, Known(true));
        return new(figure, Known(TradeDirection.Long), context.HasValue ? Known(context.Value) : Unknown<ContextualBaseContext>(),
            evidence, Known(convergence));
    }

    private static ContextualPredominanceResolutionDto Resolution(ContextualScoreInputDto input, ContextualFactorEffect predominance) =>
        new(predominance, "reviewer-1", ObservedAt.AddHours(2), "Both arguments were reviewed and the selected contextual predominance was justified.",
            input.Evidence.Factors.Where(factor => factor.Effect.Value == ContextualFactorEffect.Favorable).ToArray(),
            input.Evidence.Factors.Where(factor => factor.Effect.Value == ContextualFactorEffect.Contrary).ToArray());

    private static StructuralReferenceDto Structure(string code, CatalogReferenceDto figure) => new(
        code, Known(MapReferenceKind.WorkingLine), figure, Known(StructureDirection.Neutral), Known(60), Known(1), Known(LocationType.Support),
        new(Known(99m), Known(101m), Known(ObservedAt)), [], Known(true));

    private static ContextualFactorEvidenceDto Factor(string code, ContextualFactorEffect effect) => new(
        code, effect == ContextualFactorEffect.Favorable ? "RSI" : "OPPOSING_STRUCTURE", Known(effect), Known(63m), Known(60),
        ["primary"], "An explicitly reviewed contextual observation.");

    private static EvidenceValueDto<T> Known<T>(T value) where T : struct =>
        new(EvidenceAvailability.Present, value, ValidationSource.UserConfirmation, ObservedAt, ObservedAt.AddMinutes(1), null, null);

    private static EvidenceValueDto<T> Unknown<T>() where T : struct =>
        new(EvidenceAvailability.Unknown, null, null, null, null, null, "OBSERVATION_UNKNOWN");

    private static T RoundTrip<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
}
