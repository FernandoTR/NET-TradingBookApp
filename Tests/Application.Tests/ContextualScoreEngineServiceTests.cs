using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Application.Common;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class ContextualScoreEngineServiceTests
{
    private static readonly DateTimeOffset ObservedAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(ContextualBaseContext.Favorable, false, false, true, GradeType.A, 6, "100")]
    [InlineData(ContextualBaseContext.Favorable, false, true, true, GradeType.AB, 5, "83.33")]
    [InlineData(ContextualBaseContext.Favorable, false, false, false, GradeType.AB, 5, "83.33")]
    [InlineData(ContextualBaseContext.Neutral, true, false, false, GradeType.BA, 4, "66.67")]
    [InlineData(ContextualBaseContext.Neutral, false, false, false, GradeType.B, 3, "50")]
    [InlineData(ContextualBaseContext.Neutral, false, true, false, GradeType.BC, 2, "33.33")]
    [InlineData(ContextualBaseContext.Contrary, true, false, false, GradeType.CB, 1, "16.67")]
    [InlineData(ContextualBaseContext.Contrary, false, true, false, GradeType.C, 0, "0")]
    public void ApprovedTable_ProducesCoherentPositionDecimalScoreAndGrade(
        ContextualBaseContext context, bool favorable, bool contrary, bool convergence, GradeType grade, int rank, string shownScore)
    {
        var evaluation = Engine().Evaluate(Input(context, favorable, contrary, convergence));
        var score = evaluation.Score;

        Assert.Equal(ContextualScoreState.Complete, score.State);
        Assert.Equal(rank, score.ContextRank);
        Assert.Equal(grade, score.Grade);
        Assert.Equal(grade, evaluation.Classification.Grade);
        Assert.Equal(decimal.Parse(shownScore, CultureInfo.InvariantCulture), decimal.Round(score.Value!.Value, 2));
        Assert.Equal(evaluation.Definition.ScoreVersion, score.ScoreVersion);
        Assert.Equal(evaluation.Definition.ClassificationRulesVersion, evaluation.Classification.ClassificationRulesVersion);
        Assert.Equal(score.ExpectedComponents, score.EvaluatedComponents);
        Assert.InRange(decimal.Abs(score.Value.Value * 6m - rank * 100m), 0m, 0.000000000000000000000001m);
        if (rank is 1 or 2 or 4 or 5)
            Assert.NotEqual(decimal.Round(score.Value.Value, 2), score.Value.Value);
    }

    [Fact]
    public void SingleEvaluation_CallsTheClassifierOnceAndKeepsItsInputDefinitionAndTraceTogether()
    {
        var classifier = new CountingClassifier();
        var input = Input(ContextualBaseContext.Favorable, convergence: true);

        var evaluation = new ContextualScoreEngineService(classifier).Evaluate(input);

        Assert.Equal(1, classifier.Calls);
        Assert.Same(input, evaluation.Input);
        Assert.Same(classifier.Definition, evaluation.Definition);
        Assert.Equal(evaluation.Classification.Components, evaluation.Score.Components);
        Assert.Equal(GradeType.A, evaluation.Score.Grade);
    }

    [Fact]
    public void CompleteC_IsAKnownZeroRatherThanMissingInformation()
    {
        var score = Engine().Evaluate(Input(ContextualBaseContext.Contrary)).Score;

        Assert.Equal(ContextualScoreState.Complete, score.State);
        Assert.Equal(0, score.ContextRank);
        Assert.Equal(0m, score.Value);
        Assert.Equal(GradeType.C, score.Grade);
        Assert.True(score.ExpectedComponents > 0);
        Assert.Equal(score.ExpectedComponents, score.EvaluatedComponents);
    }

    [Fact]
    public void PartialEvaluation_RetainsKnownContextAndReasonsWithoutManufacturingATotalOrGrade()
    {
        var input = Input(ContextualBaseContext.Favorable) with { HasSufficientConvergence = Unknown<bool>() };

        var evaluation = Engine().Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, evaluation.Score.State);
        Assert.Equal(ContextualBaseContext.Favorable, evaluation.Classification.BaseContext.Value);
        Assert.Null(evaluation.Score.ContextRank);
        Assert.Null(evaluation.Score.Value);
        Assert.Null(evaluation.Score.Grade);
        Assert.True(evaluation.Score.EvaluatedComponents < evaluation.Score.ExpectedComponents);
        Assert.Contains("CONVERGENCE_ASSESSMENT_REQUIRED", evaluation.Classification.ReasonCodes);
    }

    [Fact]
    public void NonEvaluableContext_RetainsUnknownTotalsAndTheAvailableEvidence()
    {
        var input = Input(ContextualBaseContext.Favorable) with { BaseContext = Unknown<ContextualBaseContext>() };

        var evaluation = Engine().Evaluate(input);

        Assert.Equal(ContextualScoreState.NotEvaluable, evaluation.Score.State);
        Assert.Null(evaluation.Score.ContextRank);
        Assert.Null(evaluation.Score.Value);
        Assert.Null(evaluation.Score.Grade);
        Assert.Equal(input.Evidence.StructuralReferences, evaluation.Classification.StructuralReferences);
        Assert.Contains("BASE_CONTEXT_REQUIRED", evaluation.Classification.ReasonCodes);
    }

    [Fact]
    public void NeutralOpposingFactors_RemainPartialUntilAReasonedResolutionIsSupplied()
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);

        var evaluation = Engine().Evaluate(input);

        Assert.Equal(ContextualScoreState.Partial, evaluation.Score.State);
        Assert.Null(evaluation.Score.ContextRank);
        Assert.Null(evaluation.Score.Value);
        Assert.Null(evaluation.Score.Grade);
        Assert.Equal(2, evaluation.Classification.Factors.Count);
        Assert.Contains("CONTEXT_PREDOMINANCE_PENDING", evaluation.Classification.ReasonCodes);
    }

    [Theory]
    [InlineData(ContextualFactorEffect.Favorable, GradeType.BA, 4, "66.67")]
    [InlineData(ContextualFactorEffect.Contrary, GradeType.BC, 2, "33.33")]
    [InlineData(ContextualFactorEffect.Neutral, GradeType.B, 3, "50")]
    public void ManualPredominance_RecalculatesThePositionAndScoreWithoutReplacingEitherArgument(
        ContextualFactorEffect predominance, GradeType expected, int rank, string shownScore)
    {
        var input = Input(ContextualBaseContext.Neutral, favorable: true, contrary: true);
        input = input with
        {
            PredominanceResolution = new(predominance, "reviewer-1", ObservedAt.AddHours(2), "Both arguments support this reviewed predominance.",
                [input.Evidence.Factors[0]], [input.Evidence.Factors[1]])
        };
        var classifier = new CountingClassifier();

        var evaluation = new ContextualScoreEngineService(classifier).Evaluate(input);

        Assert.Equal(1, classifier.Calls);
        Assert.Equal(ContextualScoreState.Complete, evaluation.Score.State);
        Assert.Equal(rank, evaluation.Score.ContextRank);
        Assert.Equal(expected, evaluation.Score.Grade);
        Assert.Equal(decimal.Parse(shownScore, CultureInfo.InvariantCulture), decimal.Round(evaluation.Score.Value!.Value, 2));
        Assert.Equal(2, evaluation.Classification.Factors.Count);
        Assert.Same(input.PredominanceResolution, evaluation.Classification.PredominanceResolution);
    }

    [Theory]
    [InlineData(ContextualBaseContext.Favorable, false, false, true, GradeType.A)]
    [InlineData(ContextualBaseContext.Favorable, false, true, true, GradeType.AB)]
    [InlineData(ContextualBaseContext.Favorable, false, false, false, GradeType.AB)]
    [InlineData(ContextualBaseContext.Neutral, true, false, false, GradeType.BA)]
    [InlineData(ContextualBaseContext.Neutral, false, false, false, GradeType.B)]
    [InlineData(ContextualBaseContext.Neutral, false, true, false, GradeType.BC)]
    [InlineData(ContextualBaseContext.Contrary, true, false, false, GradeType.CB)]
    [InlineData(ContextualBaseContext.Contrary, false, true, false, GradeType.C)]
    public void MirroredLongShortContexts_KeepEquivalentScoresAcrossSupportAndResistance(
        ContextualBaseContext context, bool favorable, bool contrary, bool convergence, GradeType expected)
    {
        // Base context and factor effects are evidenced interpretations relative to the selected direction.
        // Mirror the structural direction, catalog figure, price reference and support/resistance role together.
        var longLocation = context == ContextualBaseContext.Neutral && contrary && !favorable
            ? LocationType.Resistance : LocationType.Support;
        var shortLocation = longLocation == LocationType.Support ? LocationType.Resistance : LocationType.Support;
        var longInput = Input(context, favorable, contrary, convergence, TradeDirection.Long, longLocation);
        var shortInput = Input(context, favorable, contrary, convergence, TradeDirection.Short, shortLocation);

        var longEvaluation = Engine().Evaluate(longInput);
        var shortEvaluation = Engine().Evaluate(shortInput);

        Assert.Equal(expected, longEvaluation.Score.Grade);
        Assert.Equal(expected, shortEvaluation.Score.Grade);
        Assert.Equal(longEvaluation.Score.ContextRank, shortEvaluation.Score.ContextRank);
        Assert.Equal(longEvaluation.Score.Value, shortEvaluation.Score.Value);
        Assert.NotEqual(longInput.Evidence.StructuralReferences[0].Location.Value, shortInput.Evidence.StructuralReferences[0].Location.Value);
        if (context != ContextualBaseContext.Neutral)
            Assert.NotEqual(longInput.Evidence.StructuralReferences[0].Direction.Value, shortInput.Evidence.StructuralReferences[0].Direction.Value);
    }

    [Theory]
    [InlineData(TradeDirection.Long, LocationType.Support)]
    [InlineData(TradeDirection.Long, LocationType.Resistance)]
    [InlineData(TradeDirection.Short, LocationType.Support)]
    [InlineData(TradeDirection.Short, LocationType.Resistance)]
    public void LocationLabelAlone_DoesNotInventAContextualModifier(TradeDirection direction, LocationType location)
    {
        var input = Input(ContextualBaseContext.Neutral, direction: direction, location: location);

        var score = Engine().Evaluate(input).Score;

        Assert.Equal(GradeType.B, score.Grade);
        Assert.Equal(50m, score.Value);
    }

    [Fact]
    public void ManualSceneryAndGradeLabelsAtTheJsonBoundary_DoNotChangeTheContextualEvidenceResult()
    {
        var input = Input(ContextualBaseContext.Contrary);
        var document = JsonSerializer.SerializeToNode(input, JsonOptions)!;
        document["sceneryId"] = 11;
        document["scenery"] = "A";
        document["grade"] = "A";
        var firstInput = document.Deserialize<ContextualScoreInputDto>(JsonOptions)!;
        document["sceneryId"] = 99;
        document["scenery"] = "C";
        document["grade"] = "C";
        var secondInput = document.Deserialize<ContextualScoreInputDto>(JsonOptions)!;

        var first = Engine().Evaluate(firstInput);
        var second = Engine().Evaluate(secondInput);

        Assert.Equal(JsonSerializer.Serialize(input, JsonOptions), JsonSerializer.Serialize(firstInput, JsonOptions));
        Assert.Equal(JsonSerializer.Serialize(firstInput, JsonOptions), JsonSerializer.Serialize(secondInput, JsonOptions));
        Assert.Equal(GradeType.C, first.Score.Grade);
        Assert.Equal(first.Score.Grade, second.Score.Grade);
        Assert.Equal(first.Score.ContextRank, second.Score.ContextRank);
        Assert.Equal(first.Score.Value, second.Score.Value);
    }

    [Fact]
    public void SameGrade_HasTheSameScoreDespiteDifferentContextualArgumentsAndCoverageCounts()
    {
        var opposed = Engine().Evaluate(Input(ContextualBaseContext.Favorable, contrary: true));
        var insufficient = Engine().Evaluate(Input(ContextualBaseContext.Favorable));

        Assert.Equal(GradeType.AB, opposed.Score.Grade);
        Assert.Equal(GradeType.AB, insufficient.Score.Grade);
        Assert.NotEqual(opposed.Classification.RuleCode, insufficient.Classification.RuleCode);
        Assert.NotEqual(opposed.Score.ExpectedComponents, insufficient.Score.ExpectedComponents);
        Assert.Equal(opposed.Score.ContextRank, insufficient.Score.ContextRank);
        Assert.Equal(opposed.Score.Value, insufficient.Score.Value);
    }

    [Fact]
    public void CoverageCounts_ReflectRuleRequirementsRatherThanThreeWeightedComponents()
    {
        var favorable = Engine().Evaluate(Input(ContextualBaseContext.Favorable, convergence: true)).Score;
        var contrary = Engine().Evaluate(Input(ContextualBaseContext.Contrary)).Score;

        Assert.True(favorable.ExpectedComponents > contrary.ExpectedComponents);
        Assert.True(contrary.ExpectedComponents > 3);
        Assert.Equal(favorable.Components.Count(component => component.Availability != EvidenceAvailability.NotApplicable), favorable.ExpectedComponents);
        Assert.Equal(contrary.Components.Count(component => component.Availability != EvidenceAvailability.NotApplicable), contrary.ExpectedComponents);
    }

    [Fact]
    public void CoverageCounts_ExcludeOnlyJustifiedInapplicabilityAndCountConfirmedAbsence()
    {
        var input = Input(ContextualBaseContext.Neutral);
        var original = new ContextualScenarioClassifier().Evaluate(input);
        var classifier = new CountingClassifier
        {
            Classify = _ => original with
            {
                State = ContextualScoreState.Partial,
                Grade = null,
                Components =
                [
                    new("KNOWN", EvidenceAvailability.Present, ValidationSource.UserConfirmation, null, ["known"]),
                    new("ABSENT", EvidenceAvailability.Absent, ValidationSource.UserConfirmation, "ABSENCE_CONFIRMED", ["absent"]),
                    new("UNKNOWN", EvidenceAvailability.Unknown, null, "NOT_DETERMINED", ["unknown"]),
                    new("JUSTIFIED_NA", EvidenceAvailability.NotApplicable, ValidationSource.DeterministicRule, "RULE_NOT_APPLICABLE", []),
                    new("UNJUSTIFIED_NA", EvidenceAvailability.NotApplicable, null, null, [])
                ]
            }
        };

        var score = new ContextualScoreEngineService(classifier).Evaluate(input).Score;

        Assert.Equal(4, score.ExpectedComponents);
        Assert.Equal(2, score.EvaluatedComponents);
        Assert.Equal(5, score.Components.Count);
        Assert.Null(score.Value);
        Assert.Null(score.Grade);
    }

    [Fact]
    public void UnifiedEvaluation_RoundTripsUnroundedDecimalsDefinitionsAndUncertainty()
    {
        var complete = Engine().Evaluate(Input(ContextualBaseContext.Neutral, favorable: true));
        var partialInput = Input(ContextualBaseContext.Neutral) with { BaseContext = Unknown<ContextualBaseContext>() };
        var unavailable = Engine().Evaluate(partialInput);

        var restored = RoundTrip(complete);
        var restoredUnavailable = RoundTrip(unavailable);

        Assert.Equal(complete.Score.Value, restored.Score.Value);
        Assert.Equal(complete.Score.Grade, restored.Score.Grade);
        Assert.Equal(complete.Score.ContextRank, restored.Score.ContextRank);
        Assert.Equal(JsonSerializer.Serialize(complete.Score.Components, JsonOptions), JsonSerializer.Serialize(restored.Score.Components, JsonOptions));
        Assert.Equal(complete.Definition.Positions, restored.Definition.Positions);
        Assert.Equal(complete.Input.Direction, restored.Input.Direction);
        Assert.Equal(complete.Input.Evidence.MainFigure.AvailableAtUtc, restored.Input.Evidence.MainFigure.AvailableAtUtc);
        Assert.Equal(ContextualScoreState.NotEvaluable, restoredUnavailable.Score.State);
        Assert.Null(restoredUnavailable.Score.Value);
        Assert.Null(restoredUnavailable.Score.ContextRank);
        Assert.Null(restoredUnavailable.Score.Grade);
    }

    [Fact]
    public void MismatchedClassificationVersion_IsNotCombinedWithAnotherScoreDefinition()
    {
        var classifier = new CountingClassifier
        {
            Classify = input => new ContextualScenarioClassifier().Evaluate(input) with { ClassificationRulesVersion = "different-rules" }
        };

        Assert.Throws<InvalidOperationException>(() => new ContextualScoreEngineService(classifier).Evaluate(Input(ContextualBaseContext.Neutral)));
        Assert.Equal(1, classifier.Calls);
    }

    [Fact]
    public void DuplicateOrdinalPositions_AreRejectedInsteadOfProducingAnIncoherentGrade()
    {
        var definition = ContextualScoreDefinitions.V1;
        var classifier = new CountingClassifier
        {
            Definition = definition with { Positions = definition.Positions.Select(position => position with { ContextRank = 3 }).ToArray() }
        };

        Assert.Throws<InvalidOperationException>(() => new ContextualScoreEngineService(classifier).Evaluate(Input(ContextualBaseContext.Neutral)));
        Assert.Equal(0, classifier.Calls);
    }

    [Fact]
    public void ClaimedCompleteClassification_WithAnUnknownRequiredComponent_IsRejected()
    {
        var classifier = new CountingClassifier
        {
            Classify = input =>
            {
                var result = new ContextualScenarioClassifier().Evaluate(input);
                return result with { Components = [.. result.Components, new("MISSING", EvidenceAvailability.Unknown, null, "MISSING_REQUIRED", [])] };
            }
        };

        Assert.Throws<InvalidOperationException>(() => new ContextualScoreEngineService(classifier).Evaluate(Input(ContextualBaseContext.Neutral)));
    }

    [Fact]
    public void NullDependenciesAndInput_AreRejectedBeforeClassification()
    {
        Assert.Throws<ArgumentNullException>(() => new ContextualScoreEngineService(null!));
        var classifier = new CountingClassifier();
        Assert.Throws<ArgumentNullException>(() => new ContextualScoreEngineService(classifier).Evaluate(null!));
        Assert.Equal(0, classifier.Calls);
    }

    [Fact]
    public void ApplicationRegistration_ResolvesTheUnifiedContextualEngine()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddApplicationServices();
        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var engine = scope.ServiceProvider.GetRequiredService<IContextualScoreEngineService>();

        Assert.Equal(50m, engine.Evaluate(Input(ContextualBaseContext.Neutral)).Score.Value);
    }

    private static ContextualScoreEngineService Engine() => new(new ContextualScenarioClassifier());

    private static ContextualScoreInputDto Input(ContextualBaseContext context, bool favorable = false, bool contrary = false,
        bool convergence = false, TradeDirection direction = TradeDirection.Long, LocationType location = LocationType.Support)
    {
        var structuralDirection = context switch
        {
            ContextualBaseContext.Favorable => direction == TradeDirection.Long ? StructureDirection.Upward : StructureDirection.Downward,
            ContextualBaseContext.Contrary => direction == TradeDirection.Long ? StructureDirection.Downward : StructureDirection.Upward,
            _ => StructureDirection.Neutral
        };
        var (concept, code, label) = structuralDirection switch
        {
            StructureDirection.Upward => (FigureConceptKind.AscendingChannel, "CA", "CANAL ASCENDENTE"),
            StructureDirection.Downward => (FigureConceptKind.DescendingChannel, "CD", "CANAL DESCENDENTE"),
            _ => (FigureConceptKind.NeutralChannel, "CN", "CANAL NEUTRO")
        };
        var reference = new CatalogReferenceDto(EvidenceAvailability.Present, code, label, Known(734),
            ValidationSource.UserConfirmation, ObservedAt, ObservedAt.AddMinutes(1), null);
        var figure = new FigureResolutionResultDto(ContextualFigureResolver.MappingVersion, reference, reference,
            new(734, code, label, true), Known(concept), [], ["FIGURE_CATALOG_RESOLVED"]);
        var lowerPrice = location == LocationType.Support ? 98m : 100m;
        var primary = new StructuralReferenceDto("primary", Known(MapReferenceKind.FigureZone), reference,
            Known(structuralDirection), Known(60), Known(2), Known(location),
            new(Known(lowerPrice), Known(lowerPrice + 2m), Known(ObservedAt)), [], Known(true));
        var higher = primary with { ReferenceCode = "higher-frame", FrameId = Known(240) };
        var factors = new List<ContextualFactorEvidenceDto>();
        if (favorable) factors.Add(Factor("favorable", ContextualFactorEffect.Favorable));
        if (contrary) factors.Add(Factor("contrary", ContextualFactorEffect.Contrary));
        var evidence = new ContextualEvidenceDto(reference, "primary", [primary, higher], Known(true),
            convergence ? [new("convergence", ["primary", "higher-frame"], Known(true), ["zone"], "Explicit favorable convergence assessment.")] : [],
            Known(true), factors, Known(true));
        return new(figure, Known(direction), Known(context), evidence, Known(convergence));
    }

    private static ContextualFactorEvidenceDto Factor(string code, ContextualFactorEffect effect) => new(code, "REVIEWED_CONTEXTUAL_FACTOR",
        Known(effect), new(EvidenceAvailability.NotApplicable, null, null, null, null, null, "QUALITATIVE_OBSERVATION"),
        Known(60), ["primary"], "The observation was interpreted relative to the proposed direction and structural context.");

    private static EvidenceValueDto<T> Known<T>(T value) where T : struct =>
        new(EvidenceAvailability.Present, value, ValidationSource.UserConfirmation, ObservedAt, ObservedAt.AddMinutes(1), null, null);

    private static EvidenceValueDto<T> Unknown<T>() where T : struct =>
        new(EvidenceAvailability.Unknown, null, null, null, null, null, "OBSERVATION_UNKNOWN");

    private static T RoundTrip<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;

    private sealed class CountingClassifier : IContextualScenarioClassifier
    {
        public ContextualScoreDefinitionDto Definition { get; init; } = ContextualScoreDefinitions.V1;
        public Func<ContextualScoreInputDto, ScenarioGradeResultDto> Classify { get; init; } = new ContextualScenarioClassifier().Evaluate;
        public int Calls { get; private set; }

        public ScenarioGradeResultDto Evaluate(ContextualScoreInputDto input)
        {
            Calls++;
            return Classify(input);
        }
    }
}
