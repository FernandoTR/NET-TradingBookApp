using System.Text.Json;
using Application.DTOs;
using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;

namespace Application.Tests;

public class ContextualScoreInputAdapterTests
{
    [Fact]
    public async Task LegacySelections_DoNotInventStructuresBaseContextOrCompletedReviews()
    {
        var adapter = Adapter(new Directions(new CatDirection { Id = 91, Code = "LONG" }));
        var order = new Order
        {
            Id = 7, CatDirectionId = 91, CatFigureId = 531, CatStageId = 2, CatFrameId = 60, CatSceneryId = 1,
            LocationType = (byte)LocationType.Support, IsTrendAligned = true, IsPivotZone = true,
            ConfirmationType = (byte)ConfirmationType.ReversalRetest, StructuralScore = 10, TotalScore = 10, Grade = "A",
            Chart = "private-chart-file.png", Comments = "private annotation",
            Date = new DateOnly(2020, 1, 1), Time = new TimeOnly(10, 0)
        };

        var input = await adapter.FromOrderAsync(order, CancellationToken.None);
        var evaluation = Engine().Evaluate(input);

        Assert.Equal(FigureConceptKind.AscendingChannel, input.MainFigure.Concept.Value);
        Assert.Equal(TradeDirection.Long, input.Direction.Value);
        Assert.Equal(ValidationSource.HistoricalEvidence, input.MainFigure.SourceReference.Source);
        Assert.Null(input.MainFigure.SourceReference.OccurredAtUtc);
        Assert.Null(input.MainFigure.SourceReference.AvailableAtUtc);
        Assert.Equal(EvidenceAvailability.Unknown, input.BaseContext.Availability);
        Assert.Null(input.Evidence.ClassificationStructureReferenceCode);
        Assert.Empty(input.Evidence.StructuralReferences);
        Assert.Empty(input.Evidence.Convergences);
        Assert.Empty(input.Evidence.Factors);
        Assert.Null(input.Evidence.StructuralReferencesComplete.Value);
        Assert.Null(input.Evidence.ConvergencesComplete.Value);
        Assert.Null(input.Evidence.FactorsComplete.Value);
        Assert.Null(input.HasSufficientConvergence.Value);
        Assert.Equal(ContextualScoreState.NotEvaluable, evaluation.Score.State);
        Assert.Null(evaluation.Score.Value);
        Assert.Null(evaluation.Score.Grade);
        Assert.Equal(10d, order.TotalScore);
        Assert.Equal("A", order.Grade);
        var json = JsonSerializer.Serialize(input);
        Assert.DoesNotContain(order.Chart, json);
        Assert.DoesNotContain(order.Comments, json);
        Assert.DoesNotContain("2020-01-01", json);
    }

    [Fact]
    public async Task PivotFigureEstablishesOnlyItsBase_WithoutTreatingLegacyFrameAsStructuralEvidence()
    {
        var order = new Order { Id = 7, CatFigureId = 734, CatDirectionId = 91, CatStageId = 2, CatFrameId = 240, IsPivotZone = false };
        var adapter = Adapter(new Directions(new CatDirection { Id = 91, Code = "SHORT" }));

        var input = await adapter.FromOrderAsync(order, CancellationToken.None);
        var evaluation = Engine().Evaluate(input);

        Assert.True(input.MainFigure.IsPivotLine);
        Assert.Equal(TradeDirection.Short, input.Direction.Value);
        Assert.Equal(ContextualScoreState.Partial, evaluation.Score.State);
        Assert.Equal(ContextualBaseContext.Neutral, evaluation.Classification.BaseContext.Value);
        Assert.Null(evaluation.Score.Grade);
        Assert.Null(evaluation.Score.Value);
        Assert.Contains("CLASSIFICATION_STRUCTURE_MISSING_OR_AMBIGUOUS", evaluation.Classification.ReasonCodes);
        Assert.Empty(input.Evidence.StructuralReferences);
    }

    [Fact]
    public async Task ManualSceneryStageAndPivotZoneFlag_DoNotSubstituteForContextualEvidence()
    {
        var adapter = Adapter(new Directions(new CatDirection { Id = 91, Code = "LONG" }));
        var order = new Order { Id = 7, CatFigureId = 734, CatDirectionId = 91, CatStageId = 1, CatSceneryId = 1, IsPivotZone = false };
        var before = Engine().Evaluate(await adapter.FromOrderAsync(order, CancellationToken.None));
        order.CatSceneryId = 999;
        order.CatStageId = 2;
        order.IsPivotZone = true;
        order.IsTrendAligned = true;
        var after = Engine().Evaluate(await adapter.FromOrderAsync(order, CancellationToken.None));

        Assert.Equal(ContextualBaseContext.Neutral, before.Classification.BaseContext.Value);
        Assert.Equal(before.Classification.BaseContext.Value, after.Classification.BaseContext.Value);
        Assert.Equal(before.Score.State, after.Score.State);
        Assert.Null(after.Score.Grade);
        Assert.Null(after.Score.Value);
        Assert.Empty(after.Input.Evidence.Factors);
    }

    [Fact]
    public async Task NormalizedExtraction_RetainsAiOriginWithoutPromotingLegacyFlagsToCompleteEvidence()
    {
        var setup = new NormalizedTradeSetupDto
        {
            DirectionId = 91, FigureId = 531, FrameId = 60, StageId = 2, SceneryId = 1,
            IsTrendAligned = true, IsPivotZone = false, LocationType = (byte)LocationType.Support, VisualConfidence = 1m
        };

        var input = await Adapter(new Directions(new CatDirection { Id = 91, Code = "LONG" }))
            .FromValidationAsync(setup, CancellationToken.None);

        Assert.Equal(ValidationSource.AiVision, input.MainFigure.SourceReference.Source);
        Assert.Equal(531, input.MainFigure.SourceReference.NormalizedId.Value);
        Assert.Null(input.MainFigure.SourceReference.AvailableAtUtc);
        Assert.Equal(ValidationSource.DeterministicRule, input.MainFigure.ResolvedReference!.NormalizedId.Source);
        Assert.Equal(ContextualScoreState.NotEvaluable, Engine().Evaluate(input).Score.State);
        Assert.Null(input.Evidence.FactorsComplete.Value);
    }

    [Theory]
    [InlineData("LONG", null, TradeDirection.Long)]
    [InlineData("LARGO", null, TradeDirection.Long)]
    [InlineData(null, "CÓMPRA", TradeDirection.Long)]
    [InlineData("SHORT", null, TradeDirection.Short)]
    [InlineData("CORTO", null, TradeDirection.Short)]
    [InlineData(null, "venta", TradeDirection.Short)]
    public async Task Direction_IsResolvedFromCatalogMeaningRatherThanNumericIds(string? code, string? description, TradeDirection expected)
    {
        var direction = new CatDirection { Id = 901, Code = code, Description = description };

        var input = await Adapter(new Directions(direction)).FromOrderAsync(new Order { CatFigureId = 734, CatDirectionId = 901 }, CancellationToken.None);

        Assert.Equal(expected, input.Direction.Value);
        Assert.Equal(ValidationSource.DeterministicRule, input.Direction.Source);
    }

    [Theory]
    [InlineData("LONG", "VENTA")]
    [InlineData("UNKNOWN", null)]
    public async Task AmbiguousOrUnrecognizedDirection_RemainsUnknown(string code, string? description)
    {
        var input = await Adapter(new Directions(new CatDirection { Id = 1, Code = code, Description = description }))
            .FromOrderAsync(new Order { CatFigureId = 734, CatDirectionId = 1 }, CancellationToken.None);

        Assert.Equal(EvidenceAvailability.Unknown, input.Direction.Availability);
        Assert.Null(input.Direction.Value);
        Assert.Equal("DIRECTION_CATALOG_NOT_DETERMINABLE", input.Direction.ReasonCode);
    }

    [Fact]
    public async Task MissingCatalogDirection_DoesNotAssumeThatIdOneMeansLong()
    {
        var input = await Adapter(new Directions()).FromOrderAsync(new Order { CatFigureId = 734, CatDirectionId = 1 }, CancellationToken.None);

        Assert.Equal(EvidenceAvailability.Unknown, input.Direction.Availability);
        Assert.Equal("DIRECTION_CATALOG_NOT_FOUND", input.Direction.ReasonCode);
    }

    [Fact]
    public async Task MissingFigureAndDirectionIds_AreUnknownRatherThanConfirmedAbsent()
    {
        var figures = new Figures();
        var directions = new Directions();
        var adapter = new ContextualScoreInputAdapter(new ContextualFigureResolver(figures), directions);

        var input = await adapter.FromOrderAsync(new Order(), CancellationToken.None);

        Assert.Equal(0, figures.Reads);
        Assert.Equal(0, directions.Reads);
        Assert.Equal(EvidenceAvailability.Unknown, input.MainFigure.SourceReference.Availability);
        Assert.Equal(EvidenceAvailability.Unknown, input.Direction.Availability);
        Assert.Equal(ContextualScoreState.NotEvaluable, Engine().Evaluate(input).Score.State);
    }

    [Fact]
    public async Task Cancellation_StopsBeforeAnyCatalogRead()
    {
        var figures = new Figures();
        var directions = new Directions();
        var adapter = new ContextualScoreInputAdapter(new ContextualFigureResolver(figures), directions);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => adapter.FromOrderAsync(new Order(), cancellation.Token));

        Assert.Equal(0, figures.Reads);
        Assert.Equal(0, directions.Reads);
    }

    private static ContextualScoreInputAdapter Adapter(Directions directions) => new(new ContextualFigureResolver(new Figures(
        new CatFigure { Id = 734, Code = "LP", Description = "LINEA PIVOTE", IsActived = true },
        new CatFigure { Id = 531, Code = "CA", Description = "CANAL ASCENDENTE", IsActived = true })), directions);

    private static ContextualScoreEngineService Engine() => new(new ContextualScenarioClassifier());

    private sealed class Directions(params CatDirection[] directions) : ICatDirectionService
    {
        public int Reads { get; private set; }
        public Task<CatDirection?> GetByIdAsync(int id) { Reads++; return Task.FromResult(directions.SingleOrDefault(direction => direction.Id == id)); }
        public Task<bool> AddAsync(CatDirection entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
        public Task<IEnumerable<CatDirection>> GetAllAsync() => throw new NotSupportedException();
        public Task<bool> UpdateAsync(CatDirection entity) => throw new NotSupportedException();
        public Task<List<GetTBAnalyticsDirectionDto>> GetTBAnalyticsDirectionAsync(ParametersTBAnalyticsDto parameters) => throw new NotSupportedException();
    }

    private sealed class Figures(params CatFigure[] figures) : ICatFigureService
    {
        public int Reads { get; private set; }
        public Task<IEnumerable<CatFigure>> GetAllAsync() { Reads++; return Task.FromResult<IEnumerable<CatFigure>>(figures); }
        public Task<CatFigure?> GetByIdAsync(int id) => throw new NotSupportedException();
        public Task<bool> AddAsync(CatFigure entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
        public Task<bool> UpdateAsync(CatFigure entity) => throw new NotSupportedException();
        public Task<List<GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(ParametersTBAnalyticsDto parameters) => throw new NotSupportedException();
    }
}
