using Application.Common;
using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class ContextualScoreSnapshotServiceTests
{
    private static readonly DateTimeOffset ObservedAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");

    [Fact]
    public async Task HistoricalRecalculation_DeduplicatesOrdersScopesOwnershipAndSavesPartialOutcomes()
    {
        var repository = new SnapshotRepository();
        repository.Orders[(11, "owner-a")] = new Infrastructure.Order
        {
            Id = 11, AuthorId = "owner-a", CatFigureId = 734, CatDirectionId = 91, CatSceneryId = 9,
            StructuralScore = 4, TotalScore = 6.25d, Grade = "AB"
        };
        repository.Orders[(12, "owner-b")] = new Infrastructure.Order { Id = 12, AuthorId = "owner-b" };
        var adapter = new SnapshotInputAdapter();
        var service = Service(repository, adapter);

        var result = await service.RecalculateHistoricalOrdersAsync("owner-a", [11, 11, 12, 404], CancellationToken.None);

        Assert.Equal(ContextualScoreSourceKind.HistoricalOrder, result.SourceKind);
        Assert.Equal(3, result.RequestedSourceCount);
        Assert.True(result.Results[0].Succeeded);
        Assert.Equal(ContextualScoreState.NotEvaluable, result.Results[0].State);
        Assert.Equal(1, result.Results[0].SnapshotId);
        Assert.False(result.Results[1].Succeeded);
        Assert.Equal("ORDER_NOT_FOUND_FOR_USER", result.Results[1].FailureCode);
        Assert.False(result.Results[2].Succeeded);
        Assert.Equal("ORDER_NOT_FOUND_FOR_USER", result.Results[2].FailureCode);
        Assert.Single(repository.Scores);
        Assert.Null(repository.Scores[0].ContextRank);
        Assert.Null(repository.Scores[0].Value);
        Assert.Null(repository.Scores[0].Grade);
        Assert.Equal("owner-a", repository.Scores[0].UserId);
        Assert.Equal(11, repository.Scores[0].OrderId);
        Assert.Null(repository.Scores[0].ValidationId);
        Assert.Null(repository.Scores[0].EvaluationRevision);
        Assert.Equal(1, adapter.OrderCalls);
        Assert.Equal(0, adapter.ValidationCalls);
    }

    [Fact]
    public async Task ReviewedRecalculation_DeduplicatesSameRevisionAndPreservesUnknownRevisions()
    {
        var repository = new SnapshotRepository();
        repository.Validations[(21, "owner-a")] = Validation(21, "owner-a", confirmedAt: ObservedAt.UtcDateTime);
        repository.Validations[(22, "owner-a")] = Validation(22, "owner-a", confirmedAt: null);
        repository.Validations[(23, "owner-b")] = Validation(23, "owner-b", confirmedAt: ObservedAt.UtcDateTime);
        var adapter = new SnapshotInputAdapter();
        var service = Service(repository, adapter);

        var result = await service.RecalculateReviewedEvaluationsAsync("owner-a",
            [new(21), new(21), new(21, 4), new(22), new(23), new(0)], CancellationToken.None);

        Assert.Equal(ContextualScoreSourceKind.ReviewedEvaluation, result.SourceKind);
        Assert.Equal(5, result.RequestedSourceCount);
        Assert.Equal(5, result.Results.Count);
        Assert.Equal(new int?[] { null, 4 }, repository.Scores.Select(snapshot => snapshot.EvaluationRevision));
        Assert.All(repository.Scores, snapshot => Assert.Equal(21, snapshot.ValidationId));
        Assert.All(repository.Scores, snapshot => Assert.Equal(ContextualScoreSourceKind.ReviewedEvaluation, snapshot.SourceKind));
        Assert.All(repository.Scores, snapshot => Assert.Null(snapshot.OrderId));
        Assert.Equal(2, adapter.ValidationCalls);
        Assert.Contains(result.Results, item => item.ValidationId == 22 && item.FailureCode == "VALIDATION_NOT_REVIEWED");
        Assert.Contains(result.Results, item => item.ValidationId == 23 && item.FailureCode == "VALIDATION_NOT_FOUND_FOR_USER");
        Assert.Contains(result.Results, item => item.ValidationId == 0 && item.FailureCode == "VALIDATION_SOURCE_INVALID");
        Assert.Null(repository.Scores[0].EvaluationRevision);
        Assert.Equal(ContextualScoreState.Complete, repository.Scores[0].State);
        Assert.Equal(GradeType.BA, repository.Scores[0].Grade);
    }

    [Fact]
    public async Task Recalculation_AppendsAnotherVersionedSnapshotWithoutUpdatingTheOriginal()
    {
        var repository = new SnapshotRepository();
        repository.Orders[(11, "owner-a")] = new Infrastructure.Order
        {
            Id = 11, AuthorId = "owner-a", StructuralScore = 4, TotalScore = 6.25d, Grade = "AB"
        };
        var service = Service(repository, new SnapshotInputAdapter());

        var original = await service.RecalculateHistoricalOrdersAsync("owner-a", [11], CancellationToken.None);
        var recalculated = await service.RecalculateHistoricalOrdersAsync("owner-a", [11], CancellationToken.None);

        Assert.Equal(1, original.Results[0].SnapshotId);
        Assert.Equal(2, recalculated.Results[0].SnapshotId);
        Assert.Equal(2, repository.Scores.Count);
        Assert.Equal(repository.Scores[0].ScoreVersion, repository.Scores[1].ScoreVersion);
        Assert.NotEqual(repository.Scores[0].Id, repository.Scores[1].Id);
        Assert.Null(repository.Scores[0].Value);
        Assert.Null(repository.Scores[1].Value);
        Assert.Equal(6.25d, repository.Orders[(11, "owner-a")].TotalScore);
        Assert.Equal("AB", repository.Orders[(11, "owner-a")].Grade);
    }

    [Fact]
    public async Task PerSourceFailure_IsReportedAndDoesNotAbortRemainingHistoricalSources()
    {
        var repository = new SnapshotRepository();
        repository.Orders[(11, "owner-a")] = new Infrastructure.Order { Id = 11, AuthorId = "owner-a" };
        repository.Orders[(12, "owner-a")] = new Infrastructure.Order { Id = 12, AuthorId = "owner-a" };
        var adapter = new SnapshotInputAdapter { FailedOrderIds = [11] };
        var service = Service(repository, adapter);

        var result = await service.RecalculateHistoricalOrdersAsync("owner-a", [11, 12], CancellationToken.None);

        Assert.False(result.Results[0].Succeeded);
        Assert.Equal("ORDER_RECALCULATION_FAILED", result.Results[0].FailureCode);
        Assert.True(result.Results[1].Succeeded);
        Assert.Single(repository.Scores);
        Assert.Equal(12, repository.Scores[0].OrderId);
    }

    [Fact]
    public async Task MetricSnapshotSave_UsesTheOwnerCheckingRepository()
    {
        var repository = new SnapshotRepository();
        var service = Service(repository, new SnapshotInputAdapter());
        var snapshot = new AiTradeValidationMetricSnapshot
        {
            UserId = "owner-a", PopulationKind = MetricPopulationKind.HistoricalOrder,
            OrderId = 11, FieldsJson = "{\"fields\":[]}", CapturedAtUtc = ObservedAt
        };

        var saved = await service.SaveMetricSnapshotAsync(snapshot, CancellationToken.None);

        Assert.Same(snapshot, saved);
        Assert.Single(repository.Metrics);
    }

    [Fact]
    public async Task CancelledRequest_StopsBeforeReadingSources()
    {
        var repository = new SnapshotRepository();
        var service = Service(repository, new SnapshotInputAdapter());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.RecalculateHistoricalOrdersAsync("owner-a", [11], cancellation.Token));

        Assert.Equal(0, repository.OrderReads);
        Assert.Empty(repository.Scores);
    }

    [Fact]
    public void ApplicationAndInfrastructureRegistration_ResolveTheRecalculationService()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TradingBookModelOnly;User Id=model;Password=model;TrustServerCertificate=True";
        builder.AddApplicationServices();
        builder.AddInfrastructureServices();
        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<ContextualScoreSnapshotService>(scope.ServiceProvider.GetRequiredService<IContextualScoreSnapshotService>());
        Assert.IsType<Infrastructure.Persistence.Repositories.ContextualScoreSnapshotRepository>(
            scope.ServiceProvider.GetRequiredService<IContextualScoreSnapshotRepository>());
        Assert.IsType<Infrastructure.Persistence.Repositories.AiTradeValidationMetricSnapshotRepository>(
            scope.ServiceProvider.GetRequiredService<IAiTradeValidationMetricSnapshotRepository>());
    }

    private static ContextualScoreSnapshotService Service(SnapshotRepository repository, SnapshotInputAdapter adapter) =>
        new(repository, repository, adapter, new TradingScoreEngineService(), new SnapshotLog());

    private static AiTradeValidation Validation(int id, string userId, DateTime? confirmedAt) => new()
    {
        Id = id, UserId = userId, DirectionId = 91, InstrumentId = 1, EntryPrice = 100m, StopLoss = 90m,
        TakeProfit = 120m, ValidationStatus = "Valid", ProviderName = "provider", ModelName = "model",
        PromptVersion = "prompt", SchemaVersion = "schema", ModelResponseJson = "{}", FinalSummary = "summary",
        CreatedAt = ObservedAt.UtcDateTime, ConfirmedAt = confirmedAt
    };

    private sealed class SnapshotRepository : IContextualScoreSnapshotRepository, IAiTradeValidationMetricSnapshotRepository
    {
        private int _scoreId;
        private int _metricId;
        public Dictionary<(int Id, string User), Infrastructure.Order> Orders { get; } = [];
        public Dictionary<(int Id, string User), AiTradeValidation> Validations { get; } = [];
        public List<ContextualScoreSnapshot> Scores { get; } = [];
        public List<AiTradeValidationMetricSnapshot> Metrics { get; } = [];
        public int OrderReads { get; private set; }

        public Task<Infrastructure.Order?> GetOrderForUserAsync(int orderId, string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OrderReads++;
            Orders.TryGetValue((orderId, userId), out var order);
            return Task.FromResult(order);
        }

        public Task<AiTradeValidation?> GetValidationForUserAsync(int validationId, string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Validations.TryGetValue((validationId, userId), out var validation);
            return Task.FromResult(validation);
        }

        public Task<ContextualScoreSnapshot> AddAsync(ContextualScoreSnapshot snapshot, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            snapshot.Id = ++_scoreId;
            Scores.Add(snapshot);
            return Task.FromResult(snapshot);
        }

        public Task<AiTradeValidationMetricSnapshot> AddAsync(AiTradeValidationMetricSnapshot snapshot, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            snapshot.Id = ++_metricId;
            Metrics.Add(snapshot);
            return Task.FromResult(snapshot);
        }

        Task<ContextualScoreSnapshot?> IContextualScoreSnapshotRepository.GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken) =>
            Task.FromResult(Scores.SingleOrDefault(snapshot => snapshot.Id == snapshotId && snapshot.UserId == userId));
        Task<AiTradeValidationMetricSnapshot?> IAiTradeValidationMetricSnapshotRepository.GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken) =>
            Task.FromResult(Metrics.SingleOrDefault(snapshot => snapshot.Id == snapshotId && snapshot.UserId == userId));
        public Task<IReadOnlyList<ContextualScoreSnapshot>> GetByIdsAsync(string userId, IReadOnlyCollection<int> snapshotIds,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ContextualScoreSnapshot>>(
                Scores.Where(snapshot => snapshot.UserId == userId && snapshotIds.Contains(snapshot.Id)).ToArray());
        public Task<IReadOnlyList<ContextualScoreSnapshot>> GetBySourceAsync(string userId, ContextualScoreSourceKind sourceKind,
            int? orderId, int? validationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ContextualScoreSnapshot>>(
                Scores.Where(snapshot => snapshot.UserId == userId && snapshot.SourceKind == sourceKind &&
                    (!orderId.HasValue || snapshot.OrderId == orderId) && (!validationId.HasValue || snapshot.ValidationId == validationId)).ToArray());
        public Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(string userId, MetricPopulationKind populationKind,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<AiTradeValidationMetricSnapshot>>(
                Metrics.Where(snapshot => snapshot.UserId == userId && snapshot.PopulationKind == populationKind).ToArray());
        public Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(string userId, AiValidationMetricFilterV2Dto filter,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<AiTradeValidationMetricSnapshot>>(
                Metrics.Where(snapshot => snapshot.UserId == userId &&
                    (!filter.PopulationKind.HasValue || snapshot.PopulationKind == filter.PopulationKind.Value)).ToArray());
    }

    private sealed class SnapshotInputAdapter : IContextualScoreInputAdapter
    {
        public HashSet<int> FailedOrderIds { get; init; } = [];
        public int OrderCalls { get; private set; }
        public int ValidationCalls { get; private set; }
        public Task<ContextualScoreInputDto> FromOrderAsync(Infrastructure.Order order, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OrderCalls++;
            if (FailedOrderIds.Contains(order.Id)) throw new InvalidOperationException("Source adapter failed.");
            return Task.FromResult(UnknownInput());
        }
        public Task<ContextualScoreInputDto> FromValidationAsync(NormalizedTradeSetupDto setup, CancellationToken cancellationToken)
        { ValidationCalls++; return Task.FromResult(CompleteInput()); }
        public Task<ContextualScoreInputDto> FromValidationAsync(AiTradeValidation validation, CancellationToken cancellationToken)
        { ValidationCalls++; return Task.FromResult(CompleteInput()); }
    }

    private sealed class SnapshotLog : ILogService
    {
        public void ErrorLog(string methodName, Exception exception) { }
        public void ErrorLog(string methodName, string message, string details) { }
        public void ActivityLog(string userId, string eventType, string description) { }
    }

    private static ContextualScoreInputDto UnknownInput()
    {
        var figure = Figure() with { Concept = Unknown<FigureConceptKind>(), ReasonCodes = ["FIGURE_CONCEPT_UNKNOWN"] };
        var structure = Structure();
        var evidence = new ContextualEvidenceDto(figure.SourceReference, "primary", [structure], Known(true), [], Known(true), [], Known(true));
        return new(figure, Known(TradeDirection.Long), Unknown<ContextualBaseContext>(), evidence, Unknown<bool>());
    }

    private static ContextualScoreInputDto CompleteInput()
    {
        var figure = Figure();
        var structure = Structure();
        var factor = new ContextualFactorEvidenceDto("factor", "RSI", Known(ContextualFactorEffect.Favorable), Known(60m), Known(60),
            ["primary"], "An explicitly reviewed favorable factor.");
        var evidence = new ContextualEvidenceDto(figure.SourceReference, "primary", [structure], Known(true), [], Known(true), [factor], Known(true));
        return new(figure, Known(TradeDirection.Long), Known(ContextualBaseContext.Neutral), evidence, Known(false));
    }

    private static FigureResolutionResultDto Figure()
    {
        var reference = new CatalogReferenceDto(EvidenceAvailability.Present, "LP", "LINEA PIVOTE", Known(4),
            ValidationSource.UserConfirmation, ObservedAt, ObservedAt, null);
        return new("figure-semantics-v1", reference, reference, new(4, "LP", "LINEA PIVOTE", true),
            Known(FigureConceptKind.PivotLine), [], ["FIGURE_CATALOG_RESOLVED"]);
    }

    private static StructuralReferenceDto Structure()
    {
        var figure = Figure().SourceReference;
        return new("primary", Known(MapReferenceKind.WorkingLine), figure, Known(StructureDirection.Neutral), Known(60),
            Known(1), Known(LocationType.Support), new(Known(99m), Known(101m), Known(ObservedAt)), [], Known(true));
    }

    private static EvidenceValueDto<T> Known<T>(T value) where T : struct =>
        new(EvidenceAvailability.Present, value, ValidationSource.UserConfirmation, ObservedAt, ObservedAt, null, null);
    private static EvidenceValueDto<T> Unknown<T>() where T : struct =>
        new(EvidenceAvailability.Unknown, null, null, null, null, null, "NOT_RECORDED");
}
