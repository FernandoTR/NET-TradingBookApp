using System.Text.Json;
using Application.Common;
using Application.DTOs;
using Application.DTOs.AiProviders;
using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class TradingScoreConsumerIntegrationTests
{
    private static readonly DateTimeOffset ObservedAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void SharedScoringFacade_ProvidesTypedEvaluationAndCompatibleLegacyCreation()
    {
        var engine = new TradingScoreEngineService();
        var order = Order();
        order.StructuralScore = null;
        order.TotalScore = null;
        order.Grade = null;

        engine.Evaluate(order);
        var evaluation = engine.Evaluate(CompleteInput());

        Assert.Equal((short)10, order.StructuralScore);
        Assert.Equal(10d, order.TotalScore);
        Assert.Equal("A", order.Grade);
        Assert.Equal(GradeType.BA, evaluation.Score.Grade);
        Assert.Equal(66.67m, decimal.Round(evaluation.Score.Value!.Value, 2));
        Assert.NotEqual(66.67m, evaluation.Score.Value.Value);
    }

    [Fact]
    public void Factory_TransportsDecimalSeparatelyAndRetainsOriginalAndSubsequentEvaluations()
    {
        var engine = new ScoreSpy();
        var factory = new AiValidationResultFactory(engine);
        var result = new AiValidationResultDto { StructuralScore = 10, TotalScore = 10, Grade = "A" };
        var input = CompleteInput();

        var originalContextual = factory.ApplyContextualTradingScore(result, input);
        var revisedInput = input with { Evidence = input.Evidence with { Factors = [] } };
        var revised = factory.ApplyContextualTradingScore(result, revisedInput);
        var restored = JsonSerializer.Deserialize<AiValidationResultDto>(JsonSerializer.Serialize(result, JsonOptions), JsonOptions)!;

        Assert.Equal(0, engine.LegacyCalls);
        Assert.Equal(2, engine.ContextualCalls);
        Assert.Equal((short)10, result.StructuralScore);
        Assert.Equal(10, result.TotalScore);
        Assert.Equal("A", result.Grade);
        Assert.Equal(2, result.ContextualEvaluations.Count);
        Assert.Same(originalContextual, result.ContextualEvaluations[0]);
        Assert.Same(revised, result.ContextualEvaluation);
        Assert.Equal(GradeType.BA, restored.ContextualEvaluations[0].Score.Grade);
        Assert.Equal(originalContextual.Score.Value, restored.ContextualEvaluations[0].Score.Value);
        Assert.Equal(GradeType.B, restored.ContextualEvaluation!.Score.Grade);
        Assert.Equal(50m, restored.ContextualEvaluation.Score.Value);
        Assert.Equal(10, restored.TotalScore);
        Assert.Equal("A", restored.Grade);
    }

    [Fact]
    public async Task FactoryAsync_UsesTheAdapterWithoutConvertingTheNewValueToTheLegacyInteger()
    {
        var adapter = new InputAdapter();
        var engine = new ScoreSpy();
        var factory = new AiValidationResultFactory(engine, adapter);
        var original = new AiValidationResultDto { StructuralScore = 4, TotalScore = 7, Grade = "B" };

        var evaluation = await factory.ApplyContextualTradingScoreAsync(original, Setup(), CancellationToken.None);

        Assert.Equal(1, adapter.ValidationCalls);
        Assert.Equal(0, adapter.OrderCalls);
        Assert.Equal(0, engine.LegacyCalls);
        Assert.Equal(1, engine.ContextualCalls);
        Assert.Equal(7, original.TotalScore);
        Assert.Equal("B", original.Grade);
        Assert.Equal(GradeType.BA, evaluation.Score.Grade);
        Assert.NotEqual(decimal.Truncate(evaluation.Score.Value!.Value), evaluation.Score.Value.Value);
        Assert.Same(evaluation, original.ContextualEvaluation);
    }

    [Fact]
    public async Task CancelledAdaptation_DoesNotAppendAnEvaluationOrOverwriteTheOriginal()
    {
        using var cancellation = new CancellationTokenSource();
        var adapter = new InputAdapter { OnRead = cancellation.Cancel };
        var engine = new ScoreSpy();
        var factory = new AiValidationResultFactory(engine, adapter);
        var result = new AiValidationResultDto { TotalScore = 10, Grade = "A" };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => factory.ApplyContextualTradingScoreAsync(result, Setup(), cancellation.Token));

        Assert.Empty(result.ContextualEvaluations);
        Assert.Equal(10, result.TotalScore);
        Assert.Equal("A", result.Grade);
        Assert.Equal(0, engine.ContextualCalls);
    }

    [Fact]
    public async Task DetachedOrderUpdate_PreservesOriginalScoresAndMergesOntoTheTrackedRow()
    {
        var existing = Order();
        var original = (existing.StructuralScore, existing.TotalScore, existing.Grade);
        var repository = new OrderStore(existing);
        var score = new ScoreSpy();
        var metrics = new Metrics();
        var service = Orders(repository, new OrderCommands(), score, metrics);
        var submitted = Order();
        submitted.StructuralScore = null;
        submitted.TotalScore = null;
        submitted.Grade = null;
        submitted.Sl = true;
        submitted.Tp1 = false;
        submitted.Tp2 = true;
        submitted.Tp3 = null;
        submitted.Target = 200m;
        submitted.Comments = "operational result";
        submitted.CatSceneryId = 999;
        submitted.CatFigureId = 900;

        var updated = await service.UpdateAsync(submitted);

        Assert.True(updated);
        Assert.Same(existing, repository.Saved);
        Assert.Equal(original, (existing.StructuralScore, existing.TotalScore, existing.Grade));
        Assert.True(existing.Sl);
        Assert.False(existing.Tp1);
        Assert.True(existing.Tp2);
        Assert.Null(existing.Tp3);
        Assert.Equal(200m, existing.Target);
        Assert.Equal("operational result", existing.Comments);
        Assert.Equal(999, existing.CatSceneryId);
        Assert.Equal(900, existing.CatFigureId);
        Assert.Equal(0, score.LegacyCalls);
        Assert.Equal(0, score.ContextualCalls);
        Assert.Equal(new[] { existing.Id }, metrics.Refreshed);
    }

    [Fact]
    public async Task DetachedOperationalSubmission_CannotReplaceTheOriginalScoreWithAnotherDefinition()
    {
        var existing = Order();
        var repository = new OrderStore(existing);
        var score = new ScoreSpy();
        var submitted = Order();
        submitted.StructuralScore = 99;
        submitted.TotalScore = 100;
        submitted.Grade = "A";

        await Orders(repository, new OrderCommands(), score, new Metrics()).UpdateAsync(submitted);

        Assert.Equal((short)4, existing.StructuralScore);
        Assert.Equal(6.25d, existing.TotalScore);
        Assert.Equal("AB", existing.Grade);
        Assert.Equal(0, score.LegacyCalls);
        Assert.Equal(0, score.ContextualCalls);
    }

    [Fact]
    public async Task ClosingAnOrder_PreservesOriginalEvaluationAndRefreshesOnlyOperationalMetrics()
    {
        var existing = Order();
        var original = (existing.StructuralScore, existing.TotalScore, existing.Grade);
        var commands = new OrderCommands();
        var score = new ScoreSpy();
        var metrics = new Metrics();
        var submitted = Order();
        submitted.StructuralScore = null;
        submitted.TotalScore = null;
        submitted.Grade = null;
        submitted.CatStatusId = 2;
        submitted.Sl = true;

        var closed = await Orders(new OrderStore(existing), commands, score, metrics).CloseOrderAsync(submitted, new Trade { OrderId = existing.Id });

        Assert.True(closed);
        Assert.Same(existing, commands.Closed);
        Assert.Equal(original, (existing.StructuralScore, existing.TotalScore, existing.Grade));
        Assert.Equal(2, existing.CatStatusId);
        Assert.True(existing.Sl);
        Assert.Equal(0, score.LegacyCalls);
        Assert.Equal(0, score.ContextualCalls);
        Assert.Equal(new[] { existing.Id }, metrics.Refreshed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedOperationalSave_DoesNotRefreshMetricsOrRunEitherScoringPath(bool closing)
    {
        var repository = new OrderStore(Order()) { UpdateResult = false };
        var commands = new OrderCommands { CloseResult = false };
        var score = new ScoreSpy();
        var metrics = new Metrics();
        var service = Orders(repository, commands, score, metrics);

        var saved = closing ? await service.CloseOrderAsync(Order(), new Trade()) : await service.UpdateAsync(Order());

        Assert.False(saved);
        Assert.Empty(metrics.Refreshed);
        Assert.Equal(0, score.LegacyCalls);
        Assert.Equal(0, score.ContextualCalls);
    }

    [Fact]
    public async Task MissingOrder_DoesNotCreateAScoreOrWriteAnUpdate()
    {
        var repository = new OrderStore(null);
        var score = new ScoreSpy();
        var metrics = new Metrics();
        var service = Orders(repository, new OrderCommands(), score, metrics);

        Assert.False(await service.UpdateAsync(Order()));
        Assert.False(await service.CloseOrderAsync(Order(), new Trade()));
        Assert.Null(repository.Saved);
        Assert.Equal(0, score.LegacyCalls);
        Assert.Empty(metrics.Refreshed);
    }

    [Fact]
    public void ExplicitOrderReevaluation_RetainsOriginalValuesVersionAndManualScenerySeparately()
    {
        var order = Order();
        var repository = new OrderStore(order);
        var score = new ScoreSpy();
        var service = Orders(repository, new OrderCommands(), score, new Metrics());

        var original = service.ReevaluateContext(order, CompleteInput());
        order.CatSceneryId = 999;
        var revised = service.ReevaluateContext(order, CompleteInput());
        var restored = JsonSerializer.Deserialize<OrderContextualScoreEvaluationDto>(JsonSerializer.Serialize(revised, JsonOptions), JsonOptions)!;

        Assert.Equal(7, original.OrderId);
        Assert.Equal(11, original.ManualSceneryId);
        Assert.Equal(999, revised.ManualSceneryId);
        Assert.Equal(new LegacyTradingScoreDto(4, 6.25d, "AB"), original.OriginalScore);
        Assert.Null(original.OriginalScore.ScoreVersion);
        Assert.Equal(original.OriginalScore, revised.OriginalScore);
        Assert.Equal(original.Evaluation.Score.Grade, revised.Evaluation.Score.Grade);
        Assert.Equal(original.Evaluation.Score.Value, revised.Evaluation.Score.Value);
        Assert.Equal(GradeType.BA, revised.Evaluation.Score.Grade);
        Assert.Equal(revised.Evaluation.Score.Value, restored.Evaluation.Score.Value);
        Assert.Equal(revised.OriginalScore, restored.OriginalScore);
        Assert.Equal(6.25d, order.TotalScore);
        Assert.Equal("AB", order.Grade);
        Assert.Null(repository.Saved);
        Assert.Equal(0, score.LegacyCalls);
        Assert.Equal(2, score.ContextualCalls);
    }

    [Fact]
    public async Task ExplicitOrderReevaluationAsync_UsesTheOrderAdapterWithoutSavingOrReplacingScores()
    {
        var order = Order();
        var repository = new OrderStore(order);
        var adapter = new InputAdapter();
        var score = new ScoreSpy();

        var evaluation = await Orders(repository, new OrderCommands(), score, new Metrics(), adapter)
            .ReevaluateContextAsync(order, CancellationToken.None);

        Assert.Equal(1, adapter.OrderCalls);
        Assert.Equal(0, adapter.ValidationCalls);
        Assert.Same(order, adapter.LastOrder);
        Assert.Equal(GradeType.BA, evaluation.Evaluation.Score.Grade);
        Assert.Null(evaluation.OriginalScore.ScoreVersion);
        Assert.Equal(6.25d, order.TotalScore);
        Assert.Null(repository.Saved);
    }

    [Fact]
    public async Task NewOrder_InitializesLegacyFieldsOnlyWhenNoOriginalResultWasSupplied()
    {
        var commands = new OrderCommands();
        var score = new ScoreSpy();
        var service = Orders(new OrderStore(null), commands, score, new Metrics());
        var empty = Order();
        empty.Id = 0;
        empty.StructuralScore = null;
        empty.TotalScore = null;
        empty.Grade = null;
        await service.AddOrderAsync(empty, new Trade());
        var imported = Order();
        imported.Id = 0;
        await service.AddOrderAsync(imported, new Trade());

        Assert.Equal(1, score.LegacyCalls);
        Assert.Equal(0, score.ContextualCalls);
        Assert.Equal(10d, empty.TotalScore);
        Assert.Equal(6.25d, imported.TotalScore);
        Assert.Equal("AB", imported.Grade);
    }

    [Fact]
    public async Task OriginalUnknownScoreFields_AreNotFilledDuringCreationOrOperationalUpdates()
    {
        var original = Order();
        original.StructuralScore = null;
        original.TotalScore = null;
        var score = new ScoreSpy();
        var service = Orders(new OrderStore(original), new OrderCommands(), score, new Metrics());

        await service.AddOrderAsync(original, new Trade());
        await service.UpdateAsync(Order());

        Assert.Null(original.StructuralScore);
        Assert.Null(original.TotalScore);
        Assert.Equal("AB", original.Grade);
        Assert.Equal(0, score.LegacyCalls);
    }

    [Fact]
    public async Task ValidationPipeline_PreservesLegacyPersistenceAndReturnsTheNewDecimalAlongsideIt()
    {
        var score = new ScoreSpy();
        var validations = new ValidationStore();
        var pipeline = new TradeValidationOrchestrator(new VisionFactory(), new Normalizer(), new Rules(), new History(),
            new AiValidationResultFactory(score, new InputAdapter()), validations);

        var result = await pipeline.ValidateAsync(new CreateAiValidationDto { UserId = "user-1" }, [], CancellationToken.None);

        Assert.Equal(1, score.LegacyCalls);
        Assert.Equal(1, score.ContextualCalls);
        Assert.Equal(10, result.TotalScore);
        Assert.Equal("A", result.Grade);
        Assert.Equal(GradeType.BA, result.ContextualEvaluation!.Score.Grade);
        Assert.Equal(66.67m, decimal.Round(result.ContextualEvaluation.Score.Value!.Value, 2));
        Assert.NotEqual(66.67m, result.ContextualEvaluation.Score.Value.Value);
        Assert.Equal(10, validations.Saved!.TotalScore);
        Assert.Equal("A", validations.Saved.Grade);
        Assert.Equal("legacy-schema-1", validations.Saved.SchemaVersion);
        Assert.Equal("legacy-prompt-1", validations.Saved.PromptVersion);
        Assert.Equal(AiValidationStatus.Valid, result.ValidationStatus);
    }

    [Fact]
    public void ApplicationRegistration_InjectsTheTypedFacadeAndAdapterAlongsideExistingConsumers()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddApplicationServices();
        builder.Services.AddSingleton<IContextualFigureResolver>(new FigureResolver());
        builder.Services.AddSingleton<ICatDirectionService>(new DirectionCatalog());
        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<ContextualScoreInputAdapter>(scope.ServiceProvider.GetRequiredService<IContextualScoreInputAdapter>());
        Assert.IsType<AiValidationResultFactory>(scope.ServiceProvider.GetRequiredService<AiValidationResultFactory>());
        Assert.Equal(GradeType.BA, scope.ServiceProvider.GetRequiredService<ITradingScoreEngineService>().Evaluate(CompleteInput()).Score.Grade);
    }

    private static OrdersService Orders(OrderStore repository, OrderCommands commands, ScoreSpy score, Metrics metrics, InputAdapter? adapter = null) =>
        new(repository, commands, null!, null!, null!, null!, null!, score, null!, metrics, adapter);

    private static Order Order() => new()
    {
        Id = 7, AuthorId = "user-1", AccountId = 10, CatCategoryId = 1, CatInstrumentId = 1, CatStageId = 2,
        CatFigureId = 734, CatDirectionId = 91, CatFrameId = 60, CatTriggerId = 5, CatSceneryId = 11, CatDayId = 1,
        CatStatusId = 1, Date = new DateOnly(2026, 10, 23), Time = new TimeOnly(10, 0), Chart = "",
        LocationType = (byte)LocationType.Support, IsTrendAligned = true, IsPivotZone = false,
        ConfirmationType = (byte)ConfirmationType.ReversalRetest, StructuralScore = 4, TotalScore = 6.25d, Grade = "AB"
    };

    private static NormalizedTradeSetupDto Setup() => new()
    {
        InstrumentId = 1, DirectionId = 91, FigureId = 734, StageId = 2, FrameId = 60, TriggerId = 5, SceneryId = 11,
        LocationType = (byte)LocationType.Support, ConfirmationType = (byte)ConfirmationType.ReversalRetest,
        IsTrendAligned = true, IsPivotZone = false, RiskRewardRatio = 2m, EntryPrice = 100m, StopLoss = 90m, TakeProfit = 120m
    };

    private static ContextualScoreInputDto CompleteInput()
    {
        var reference = new CatalogReferenceDto(EvidenceAvailability.Present, "LP", "LINEA PIVOTE", Known(734),
            ValidationSource.UserConfirmation, ObservedAt, ObservedAt.AddMinutes(1), null);
        var figure = new FigureResolutionResultDto(ContextualFigureResolver.MappingVersion, reference, reference,
            new(734, "LP", "LINEA PIVOTE", true), Known(FigureConceptKind.PivotLine), [], ["FIGURE_CATALOG_RESOLVED"]);
        var structure = new StructuralReferenceDto("primary", Known(MapReferenceKind.WorkingLine), reference, Known(StructureDirection.Neutral),
            Known(60), Known(1), Known(LocationType.Support), new(Known(99m), Known(101m), Known(ObservedAt)), [], Known(true));
        var factor = new ContextualFactorEvidenceDto("favorable", "RSI", Known(ContextualFactorEffect.Favorable), Known(63m), Known(60),
            ["primary"], "A reviewed favorable observation.");
        var evidence = new ContextualEvidenceDto(reference, "primary", [structure], Known(true), [], Known(true), [factor], Known(true));
        return new(figure, Known(TradeDirection.Long), Known(ContextualBaseContext.Neutral), evidence, Known(false));
    }

    private static EvidenceValueDto<T> Known<T>(T value) where T : struct =>
        new(EvidenceAvailability.Present, value, ValidationSource.UserConfirmation, ObservedAt, ObservedAt.AddMinutes(1), null, null);

    private sealed class ScoreSpy : ITradingScoreEngineService
    {
        public int LegacyCalls { get; private set; }
        public int ContextualCalls { get; private set; }
        public void Evaluate(Order order) { LegacyCalls++; new TradingScoreEngineService().Evaluate(order); }
        public ContextualScoreEvaluationDto Evaluate(ContextualScoreInputDto input) { ContextualCalls++; return new TradingScoreEngineService().Evaluate(input); }
    }

    private sealed class InputAdapter : IContextualScoreInputAdapter
    {
        public int OrderCalls { get; private set; }
        public int ValidationCalls { get; private set; }
        public Order? LastOrder { get; private set; }
        public Action? OnRead { get; init; }
        public Task<ContextualScoreInputDto> FromOrderAsync(Order order, CancellationToken cancellationToken)
        {
            OrderCalls++; LastOrder = order; OnRead?.Invoke(); cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CompleteInput());
        }
        public Task<ContextualScoreInputDto> FromValidationAsync(NormalizedTradeSetupDto setup, CancellationToken cancellationToken)
        {
            ValidationCalls++; OnRead?.Invoke(); cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CompleteInput());
        }
        public Task<ContextualScoreInputDto> FromValidationAsync(AiTradeValidation validation, CancellationToken cancellationToken)
        {
            ValidationCalls++; OnRead?.Invoke(); cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CompleteInput());
        }
    }

    private sealed class OrderStore(Order? existing) : IGenericRepository<Order>
    {
        public Order? Saved { get; private set; }
        public bool UpdateResult { get; init; } = true;
        public Task<Order?> GetByIdAsync(int id, QueryOptions<Order>? options = null) => Task.FromResult(existing?.Id == id ? existing : null);
        public Task<bool> UpdateAsync(Order entity) { Saved = entity; return Task.FromResult(UpdateResult); }
        public Task<IEnumerable<Order>> GetAllAsync(QueryOptions<Order>? options = null) => throw new NotSupportedException();
        public Task<bool> AddAsync(Order entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
    }

    private sealed class OrderCommands : IOrdersRepository
    {
        public Order? Closed { get; private set; }
        public bool CloseResult { get; init; } = true;
        public Task<bool> CloseOrderAsync(Order entity, Trade trade) { Closed = entity; return Task.FromResult(CloseResult); }
        public Task<(bool, int)> AddOrderAsync(Order entity, Trade trade) => Task.FromResult((true, entity.Id > 0 ? entity.Id : 55));
        public Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters) => throw new NotSupportedException();
    }

    private sealed class Metrics : IAiTradeValidationMetricService
    {
        public List<int> Refreshed { get; } = [];
        public Task RefreshOrderOutcomeAsync(int orderId, CancellationToken cancellationToken) { Refreshed.Add(orderId); return Task.CompletedTask; }
        public Task CreateInitialMetricAsync(int validationId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AiValidationMetricSummaryDto> GetSummaryAsync(string userId, AiValidationMetricFilterDto filter, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AiValidationMetricSummaryV2Dto> GetSummaryV2Async(string userId, AiValidationMetricFilterV2Dto filter, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class VisionFactory : IAiVisionClientFactory
    {
        public Task<AiVisionClientSelection> CreateActiveClientAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new AiVisionClientSelection { Client = new Vision(), Configuration = new AiProviderRuntimeConfiguration { ModelName = "test-model" } });
    }

    private sealed class Vision : IAiVisionClient
    {
        public string ProviderName => "Test";
        public string PromptVersion => "legacy-prompt-1";
        public string SchemaVersion => "legacy-schema-1";
        public Task<AiVisionExtractionDto> ExtractSetupAsync(CreateAiValidationDto request, IReadOnlyList<AiValidationImageInputDto> images,
            AiProviderRuntimeConfiguration configuration, CancellationToken cancellationToken) => Task.FromResult(new AiVisionExtractionDto());
    }

    private sealed class Normalizer : ITradeSetupNormalizer
    {
        public Task<NormalizedTradeSetupDto> NormalizeAsync(CreateAiValidationDto request, AiVisionExtractionDto extraction, CancellationToken cancellationToken) => Task.FromResult(Setup());
    }

    private sealed class Rules : IStrategyRuleEngine
    {
        public IReadOnlyList<AiValidationRuleResultDto> Evaluate(NormalizedTradeSetupDto setup) =>
            [new() { RuleCode = "LEGACY", RuleName = "Legacy rule", Result = ValidationRuleResult.Passed, Source = ValidationSource.DeterministicRule }];
    }

    private sealed class History : IHistoricalEvidenceService
    {
        public Task<HistoricalEvidenceDto?> GetEvidenceAsync(NormalizedTradeSetupDto setup, CancellationToken cancellationToken) => Task.FromResult<HistoricalEvidenceDto?>(null);
    }

    private sealed class ValidationStore : IAiTradeValidationRepository
    {
        public AiTradeValidation? Saved { get; private set; }
        public Task<int> SaveCompletedAsync(AiTradeValidation validation, IEnumerable<AiTradeValidationRule> rules, CancellationToken cancellationToken)
        { Saved = validation; return Task.FromResult(42); }
        public Task<AiTradeValidation?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<AiTradeValidation>> GetByUserAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<AiTradeValidation>> GetCompletedByUserAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> ConfirmAsync(ConfirmedAiValidationDto confirmation, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> LinkOrderAsync(int validationId, int orderId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FigureResolver : IContextualFigureResolver
    {
        public Task<FigureResolutionResultDto> ResolveAsync(CatalogReferenceDto reference, CancellationToken cancellationToken) => Task.FromResult(CompleteInput().MainFigure);
    }

    private sealed class DirectionCatalog : ICatDirectionService
    {
        public Task<CatDirection?> GetByIdAsync(int id) => Task.FromResult<CatDirection?>(new CatDirection { Id = id, Code = "LONG" });
        public Task<bool> AddAsync(CatDirection entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
        public Task<IEnumerable<CatDirection>> GetAllAsync() => throw new NotSupportedException();
        public Task<bool> UpdateAsync(CatDirection entity) => throw new NotSupportedException();
        public Task<List<GetTBAnalyticsDirectionDto>> GetTBAnalyticsDirectionAsync(ParametersTBAnalyticsDto parameters) => throw new NotSupportedException();
    }
}
