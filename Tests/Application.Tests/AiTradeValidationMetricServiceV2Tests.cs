using System.Text.Json;
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

public class AiTradeValidationMetricServiceV2Tests
{
    private static readonly DateTimeOffset Captured = DateTimeOffset.Parse("2026-10-23T10:00:00Z");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Summary_DeduplicatesRevisionsAndOrderRecalculationsGroupsVersionsAndUsesOnlyObservedOutcomes()
    {
        var metrics = new MetricSnapshotStore(
            [
                Reviewed(ValidationId: 100, Revision: 1, ScoreId: 1, CapturedAt: Captured.AddHours(9), Model: "old-model",
                    ManualScenario: "C", Verdict: SetupVerdict.Invalid, Fields: [Field("legacy", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true)]),
                Reviewed(ValidationId: 100, Revision: 2, ScoreId: 2, CapturedAt: Captured.AddHours(2), Model: "vision-model",
                    ManualScenario: "C", Verdict: SetupVerdict.Pending, Fields: [Field("stale-recalc", AiValidationFieldChangeKind.NoReview, EvidenceAvailability.Unknown, null)]),
                Reviewed(ValidationId: 100, Revision: 2, ScoreId: 3, CapturedAt: Captured.AddHours(4), Model: "vision-model",
                    ManualScenario: "ba", Verdict: SetupVerdict.Valid,
                    Fields: [Field("matched", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true),
                        Field("corrected", AiValidationFieldChangeKind.Correction, EvidenceAvailability.Present, false)],
                    notApplicable: 1),
                Reviewed(ValidationId: 101, Revision: null, ScoreId: 4, CapturedAt: Captured.AddHours(5), Model: "vision-model",
                    ManualScenario: "A", Verdict: null,
                    Fields: [Field("completed", AiValidationFieldChangeKind.ManualCompletion, EvidenceAvailability.Unknown, null),
                        Field("unreviewed", AiValidationFieldChangeKind.NoReview, EvidenceAvailability.Present, null)]),
                Historical(OrderId: 200, ScoreId: 5, CapturedAt: Captured.AddHours(6), outcome: Outcome(observedAt: Captured.AddHours(1), sl: true, tp1: false),
                    fields: [Field("not-ai-review-data", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true)]),
                Historical(OrderId: 200, ScoreId: 6, CapturedAt: Captured.AddHours(8), outcome: Outcome(observedAt: Captured.AddHours(3), sl: false, tp1: true, tp2: false))
            ]);
        var scores = new ScoreSnapshotStore(
            Score(1, ContextualScoreSourceKind.ReviewedEvaluation, null, 100, 1, "score-v1", 0, GradeType.C, 0m),
            Score(2, ContextualScoreSourceKind.ReviewedEvaluation, null, 100, 2, "score-v1", 3, GradeType.B, 50m),
            Score(3, ContextualScoreSourceKind.ReviewedEvaluation, null, 100, 2, "score-v1", 4, GradeType.BA, 100m * 4m / 6m),
            Score(4, ContextualScoreSourceKind.ReviewedEvaluation, null, 101, null, "score-v1", 4, GradeType.BA, 100m * 4m / 6m),
            Score(5, ContextualScoreSourceKind.HistoricalOrder, 200, null, null, "score-v1", 1, GradeType.CB, 100m / 6m),
            Score(6, ContextualScoreSourceKind.HistoricalOrder, 200, null, null, "score-v1", 0, GradeType.C, 0m));
        var service = CreateService(metrics, scores);

        var summary = await service.GetSummaryV2Async("user-1", new(), CancellationToken.None);

        Assert.Equal(3, summary.SampleSize);
        Assert.Equal(2, summary.DistinctValidationCount);
        Assert.Equal(1, summary.DistinctOrderCount);
        Assert.Equal(new[] { MetricPopulationKind.HistoricalOrder, MetricPopulationKind.ReviewedEvaluation }, summary.IncludedPopulations);
        Assert.Equal(2, summary.Groups.Count);

        var reviewed = Assert.Single(summary.Groups, group => group.Versions.PopulationKind == MetricPopulationKind.ReviewedEvaluation);
        var historical = Assert.Single(summary.Groups, group => group.Versions.PopulationKind == MetricPopulationKind.HistoricalOrder);
        Assert.Equal(0, historical.FieldMetrics.Coverage.Denominator);
        Assert.Null(historical.FieldMetrics.Coverage.Percentage);
        Assert.Equal(2, reviewed.SampleSize);
        Assert.Equal("vision-model", reviewed.Versions.ModelName);
        Assert.Equal(3, reviewed.FieldMetrics.Coverage.Numerator);
        Assert.Equal(4, reviewed.FieldMetrics.Coverage.Denominator);
        Assert.Equal(75m, reviewed.FieldMetrics.Coverage.Percentage);
        Assert.Equal(1, reviewed.FieldMetrics.Concordance.Numerator);
        Assert.Equal(2, reviewed.FieldMetrics.Concordance.Denominator);
        Assert.Equal(50m, reviewed.FieldMetrics.Concordance.Percentage);
        Assert.Equal(1, reviewed.FieldMetrics.HumanCorrections.Numerator);
        Assert.Equal(2, reviewed.FieldMetrics.HumanCorrections.Denominator);
        Assert.Equal(50m, reviewed.FieldMetrics.HumanCorrections.Percentage);
        Assert.Equal(1, reviewed.FieldMetrics.ManualCompletions.Numerator);
        Assert.Equal(100m, reviewed.FieldMetrics.ManualCompletions.Percentage);
        Assert.Equal(1, reviewed.FieldMetrics.NoReviewCount);
        Assert.Equal(1, reviewed.NotApplicableFieldCount);
        Assert.Equal(1, reviewed.ScenarioGradeConcordance.Numerator);
        Assert.Equal(2, reviewed.ScenarioGradeConcordance.Denominator);
        Assert.Equal(50m, reviewed.ScenarioGradeConcordance.Percentage);
        Assert.Equal(SetupVerdict.Valid, Assert.Single(reviewed.VerdictDistribution, item => item.Count == 1).Verdict);
        Assert.All(reviewed.VerdictDistribution, item => Assert.Equal(1, item.Denominator));

        var outcome = Assert.Single(summary.OutcomeRelationships);
        Assert.Equal(MetricPopulationKind.HistoricalOrder, outcome.PopulationKind);
        Assert.Equal(1, outcome.OrderCount);
        Assert.Equal("score-v1", outcome.ScoreVersion);
        Assert.Equal(GradeType.C, outcome.Grade);
        Assert.Equal(0m, outcome.ScoreValue);
        Assert.Empty(outcome.OutcomeCodes);
        Assert.Equal(Captured.AddHours(3), outcome.ObservedAtUtcMin);
        Assert.Equal(Captured.AddHours(3), outcome.ObservedAtUtcMax);
        Assert.Equal(0, outcome.ReachedSl.Numerator);
        Assert.Equal(1, outcome.ReachedSl.Denominator);
        Assert.Equal(100m, outcome.ReachedTp1.Percentage);
        Assert.Equal(0m, outcome.ReachedTp2.Percentage);
        Assert.Null(outcome.ReachedTp3.Percentage);
    }

    [Fact]
    public async Task Summary_UsesLatestKnownRevisionAndCanExplicitlyIncludeAllRecordedRevisions()
    {
        var metrics = new MetricSnapshotStore(
            [
                Reviewed(ValidationId: 100, Revision: 1, ScoreId: 1, CapturedAt: Captured.AddDays(1), Model: "model",
                    ManualScenario: "A", Verdict: SetupVerdict.Invalid, Fields: [Field("revision-1", AiValidationFieldChangeKind.Correction, EvidenceAvailability.Present, false)]),
                Reviewed(ValidationId: 100, Revision: 2, ScoreId: 2, CapturedAt: Captured, Model: "model",
                    ManualScenario: "BA", Verdict: SetupVerdict.Valid, Fields: [Field("revision-2", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true)])
            ]);
        var scores = new ScoreSnapshotStore(
            Score(1, ContextualScoreSourceKind.ReviewedEvaluation, null, 100, 1, "v1", 6, GradeType.A, 100m),
            Score(2, ContextualScoreSourceKind.ReviewedEvaluation, null, 100, 2, "v1", 4, GradeType.BA, 100m * 4m / 6m));
        var service = CreateService(metrics, scores);

        var latest = await service.GetSummaryV2Async("user-1", new(), CancellationToken.None);
        var all = await service.GetSummaryV2Async("user-1", new(IncludeAllRevisions: true), CancellationToken.None);
        var obsoleteModel = await service.GetSummaryV2Async("user-1", new(ModelName: "old-model"), CancellationToken.None);

        Assert.Single(latest.Groups);
        Assert.Equal(1, latest.SampleSize);
        Assert.Equal("revision-2", Assert.Single(latest.Groups[0].FieldMetrics.Fields).FieldCode);
        Assert.Equal(2, all.SampleSize);
        Assert.Equal(2, all.Groups[0].SampleSize);
        Assert.Equal(1, all.DistinctValidationCount);
        Assert.Equal(0, obsoleteModel.SampleSize);
        Assert.Empty(obsoleteModel.Groups);
    }

    [Fact]
    public async Task Summary_AppliesUserPopulationVersionDimensionAndDateFiltersBeforeAggregation()
    {
        var metrics = new MetricSnapshotStore(
            [
                Reviewed(100, 1, 1, Captured, "model-a", "A", null, [Field("allowed", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true)],
                    instrumentId: 2, stageId: 3, triggerId: 4, userId: "user-1"),
                Reviewed(101, 1, 2, Captured.AddDays(2), "model-b", "A", null, [Field("other-model", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true)],
                    instrumentId: 2, stageId: 3, triggerId: 4, userId: "user-1"),
                Reviewed(102, 1, 3, Captured, "model-a", "A", null, [Field("other-user", AiValidationFieldChangeKind.NoChange, EvidenceAvailability.Present, true)],
                    instrumentId: 2, stageId: 3, triggerId: 4, userId: "user-2")
            ]);
        var scores = new ScoreSnapshotStore(
            Score(1, ContextualScoreSourceKind.ReviewedEvaluation, null, 100, 1, "v1", 3, GradeType.B, 50m),
            Score(2, ContextualScoreSourceKind.ReviewedEvaluation, null, 101, 1, "v1", 3, GradeType.B, 50m),
            Score(3, ContextualScoreSourceKind.ReviewedEvaluation, null, 102, 1, "v1", 3, GradeType.B, 50m, userId: "user-2"));
        var service = CreateService(metrics, scores);

        var summary = await service.GetSummaryV2Async("user-1", new(
            PopulationKind: MetricPopulationKind.ReviewedEvaluation,
            ModelName: "model-a",
            FromUtc: Captured,
            ToUtc: Captured,
            InstrumentId: 2,
            StageId: 3,
            TriggerId: 4,
            MetricsVersion: "field-metrics-v2"), CancellationToken.None);

        Assert.Equal(1, summary.SampleSize);
        Assert.Equal(new[] { MetricPopulationKind.ReviewedEvaluation }, summary.IncludedPopulations);
        Assert.Single(summary.Groups);
        Assert.Equal("model-a", summary.Groups[0].Versions.ModelName);
        Assert.Equal("allowed", Assert.Single(summary.Groups[0].FieldMetrics.Fields).FieldCode);
        Assert.Empty(summary.OutcomeRelationships);
    }

    [Fact]
    public async Task Summary_PreservesUnknownVersionsAndDoesNotCountManualScenarioWithoutCalculatedGrade()
    {
        var metrics = new MetricSnapshotStore(
            [Reviewed(100, null, 1, Captured, "model", "BA", null,
                [Field("unassessed", AiValidationFieldChangeKind.NoReview, EvidenceAvailability.Unknown, null)],
                scoreVersion: null, graphical: null)]);
        var scores = new ScoreSnapshotStore();

        var summary = await CreateService(metrics, scores).GetSummaryV2Async("user-1", new(), CancellationToken.None);

        var group = Assert.Single(summary.Groups);
        Assert.Null(group.Versions.ScoreVersion);
        Assert.Equal("model", group.Versions.ModelName);
        Assert.Null(group.ScenarioGradeConcordance.Percentage);
        Assert.Equal(0, group.ScenarioGradeConcordance.Denominator);
        Assert.All(group.VerdictDistribution, verdict => Assert.Null(verdict.Percentage));
        Assert.Null(summary.OutcomeRelationships.SingleOrDefault());
    }

    [Fact]
    public async Task Summary_DistinguishesPopulationsAndDoesNotInferSlTpSequenceFromFlags()
    {
        var metrics = new MetricSnapshotStore(
            [
                Historical(200, 5, Captured, Outcome(Captured, sl: true, tp1: true, tp2: null, tp3: null, code: null)),
                Reviewed(100, 1, 6, Captured.AddMinutes(1), "model", "B", SetupVerdict.Pending,
                    [Field("field", AiValidationFieldChangeKind.NoReview, EvidenceAvailability.Unknown, null)], orderId: 200,
                    scoreVersion: "review-v", outcome: Outcome(Captured.AddMinutes(1), sl: null, tp1: true, tp2: true, tp3: false, code: "TP2"))
            ]);
        var scores = new ScoreSnapshotStore(
            Score(5, ContextualScoreSourceKind.HistoricalOrder, 200, null, null, "hist-v", 3, GradeType.B, 50m),
            Score(6, ContextualScoreSourceKind.ReviewedEvaluation, 200, 100, 1, "review-v", 2, GradeType.BC, 100m / 3m));

        var summary = await CreateService(metrics, scores).GetSummaryV2Async("user-1", new(), CancellationToken.None);

        Assert.Equal(2, summary.SampleSize);
        Assert.Equal(2, summary.Groups.Count);
        var outcome = Assert.Single(summary.OutcomeRelationships);
        Assert.Equal(MetricPopulationKind.ReviewedEvaluation, outcome.PopulationKind);
        Assert.Equal("review-v", outcome.ScoreVersion);
        Assert.Equal(1, outcome.OrderCount);
        Assert.Equal(new[] { "TP2" }, outcome.OutcomeCodes);
        Assert.Equal(1, outcome.ReachedTp1.Numerator);
        Assert.Equal(1, outcome.ReachedTp2.Numerator);
        Assert.Equal(0, outcome.ReachedTp3.Numerator);
        Assert.Equal(Captured.AddMinutes(1), outcome.ObservedAtUtcMin);
        Assert.Equal(SetupVerdict.Pending, Assert.Single(summary.Groups.Single(group => group.Versions.PopulationKind ==
            MetricPopulationKind.ReviewedEvaluation).VerdictDistribution, item => item.Count > 0).Verdict);
    }

    [Fact]
    public async Task Summary_RequiresOwnerAndConfiguredSnapshotRepositories()
    {
        var service = new AiTradeValidationMetricService(new LegacyRepository(), new MetricLog());

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetSummaryV2Async(" ", new(), CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetSummaryV2Async("user-1", new(), CancellationToken.None));
    }

    [Fact]
    public void MetricService_ResolvesSnapshotRepositoriesThroughApplicationAndInfrastructureRegistration()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            "Server=localhost;Database=TradingBookModelOnly;User Id=model;Password=model;TrustServerCertificate=True";
        builder.AddApplicationServices();
        builder.AddInfrastructureServices();
        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<AiTradeValidationMetricService>(scope.ServiceProvider.GetRequiredService<IAiTradeValidationMetricService>());
    }

    private static AiTradeValidationMetricService CreateService(MetricSnapshotStore metrics, ScoreSnapshotStore scores) =>
        new(new LegacyRepository(), new MetricLog(), metrics, scores);

    private static AiTradeValidationMetricSnapshot Reviewed(int ValidationId, int? Revision, int ScoreId,
        DateTimeOffset CapturedAt, string? Model, string? ManualScenario, SetupVerdict? Verdict,
        IReadOnlyList<AiValidationFieldMetricDto> Fields, int notApplicable = 0, int instrumentId = 2,
        int stageId = 3, int triggerId = 4, string userId = "user-1", int? orderId = 200,
        string? scoreVersion = "score-v1", AiValidationOutcomeObservationDto? outcome = null,
        AiValidationMetricGraphicalSnapshotDto? graphical = null) => new()
    {
        Id = ScoreId + 1000,
        PopulationKind = MetricPopulationKind.ReviewedEvaluation,
        UserId = userId,
        ValidationId = ValidationId,
        OrderId = orderId,
        EvaluationRevision = Revision,
        ContextualScoreSnapshotId = ScoreId,
        InstrumentId = instrumentId,
        StageId = stageId,
        TriggerId = triggerId,
        StrategyVersion = "strategy-v1",
        EvidenceSchemaVersion = "evidence-v2",
        GraphicalRulesVersion = "graphical-v1",
        ClassificationRulesVersion = "contextual-v1",
        ScoreVersion = scoreVersion,
        MetricsVersion = "field-metrics-v2",
        PromptVersion = "prompt-v1",
        ExtractionSchemaVersion = "extract-v1",
        ProviderName = "provider",
        ModelName = Model,
        FieldsJson = JsonSerializer.Serialize(new AiValidationMetricFieldsSnapshotDto(Fields, notApplicable), JsonOptions),
        GraphicResultsJson = JsonSerializer.Serialize(graphical ?? new AiValidationMetricGraphicalSnapshotDto(
            Verdict.HasValue ? Evaluation(Verdict.Value) : null, ManualScenario), JsonOptions),
        OutcomeObservationsJson = outcome is null ? null : JsonSerializer.Serialize(outcome, JsonOptions),
        CapturedAtUtc = CapturedAt
    };

    private static AiTradeValidationMetricSnapshot Historical(int OrderId, int ScoreId, DateTimeOffset CapturedAt,
        AiValidationOutcomeObservationDto? outcome, IReadOnlyList<AiValidationFieldMetricDto>? fields = null) => new()
    {
        Id = ScoreId + 2000,
        PopulationKind = MetricPopulationKind.HistoricalOrder,
        UserId = "user-1",
        OrderId = OrderId,
        ContextualScoreSnapshotId = ScoreId,
        MetricsVersion = "field-metrics-v2",
        ScoreVersion = "score-v1",
        FieldsJson = JsonSerializer.Serialize(new AiValidationMetricFieldsSnapshotDto(fields ?? [], 0), JsonOptions),
        OutcomeObservationsJson = outcome is null ? null : JsonSerializer.Serialize(outcome, JsonOptions),
        CapturedAtUtc = CapturedAt
    };

    private static SetupEvaluationResultDto Evaluation(SetupVerdict verdict) => new("evaluation-1", "evidence-1", new())
    {
        Zone = new(SetupConditionKind.Zone) { State = verdict == SetupVerdict.Valid ? SetupConditionState.Passed : SetupConditionState.Pending },
        Trigger = new(SetupConditionKind.Trigger) { State = verdict == SetupVerdict.Valid ? SetupConditionState.Passed : SetupConditionState.Pending },
        Ratio = new(SetupConditionKind.Ratio) { State = verdict == SetupVerdict.Valid ? SetupConditionState.Passed : SetupConditionState.Pending }
    };

    private static ContextualScoreSnapshot Score(int id, ContextualScoreSourceKind sourceKind, int? orderId,
        int? validationId, int? revision, string version, int? rank, GradeType? grade, decimal? value, string userId = "user-1") => new()
    {
        Id = id,
        UserId = userId,
        SourceKind = sourceKind,
        OrderId = orderId,
        ValidationId = validationId,
        EvaluationRevision = revision,
        ScoreVersion = version,
        State = grade.HasValue ? ContextualScoreState.Complete : ContextualScoreState.Partial,
        ContextRank = rank,
        Grade = grade,
        Value = value,
        DefinitionJson = "{}",
        InputSnapshotJson = "{}",
        ComponentsJson = "{}",
        CalculatedAtUtc = Captured
    };

    private static AiValidationOutcomeObservationDto Outcome(DateTimeOffset observedAt, bool? sl = null, bool? tp1 = null,
        bool? tp2 = null, bool? tp3 = null, string? code = null) => new(observedAt, sl, tp1, tp2, tp3, code);

    private static AiValidationFieldMetricDto Field(string code, AiValidationFieldChangeKind change,
        EvidenceAvailability ai, bool? matches) => new(code, ai, EvidenceAvailability.Present,
            ValidationSource.AiVision, ValidationSource.UserConfirmation, matches, change, null);

    private sealed class MetricSnapshotStore : IAiTradeValidationMetricSnapshotRepository
    {
        private readonly AiTradeValidationMetricSnapshot[] _snapshots;

        public MetricSnapshotStore(params AiTradeValidationMetricSnapshot[] snapshots) => _snapshots = snapshots;

        public Task<AiTradeValidationMetricSnapshot> AddAsync(AiTradeValidationMetricSnapshot snapshot, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AiTradeValidationMetricSnapshot?> GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(string userId, MetricPopulationKind populationKind, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AiTradeValidationMetricSnapshot>>(_snapshots.Where(snapshot => snapshot.UserId == userId && snapshot.PopulationKind == populationKind).ToArray());
        public Task<IReadOnlyList<AiTradeValidationMetricSnapshot>> GetForUserAsync(string userId, AiValidationMetricFilterV2Dto filter, CancellationToken cancellationToken)
        {
            IEnumerable<AiTradeValidationMetricSnapshot> query = _snapshots.Where(snapshot => snapshot.UserId == userId);
            if (filter.PopulationKind.HasValue) query = query.Where(snapshot => snapshot.PopulationKind == filter.PopulationKind.Value);
            if (filter.ProviderName is not null) query = query.Where(snapshot => snapshot.ProviderName == filter.ProviderName);
            if (filter.ModelName is not null) query = query.Where(snapshot => snapshot.ModelName == filter.ModelName);
            if (filter.FromUtc.HasValue) query = query.Where(snapshot => snapshot.CapturedAtUtc >= filter.FromUtc.Value);
            if (filter.ToUtc.HasValue) query = query.Where(snapshot => snapshot.CapturedAtUtc <= filter.ToUtc.Value);
            if (filter.InstrumentId.HasValue) query = query.Where(snapshot => snapshot.InstrumentId == filter.InstrumentId);
            if (filter.StageId.HasValue) query = query.Where(snapshot => snapshot.StageId == filter.StageId);
            if (filter.TriggerId.HasValue) query = query.Where(snapshot => snapshot.TriggerId == filter.TriggerId);
            if (filter.MetricsVersion is not null) query = query.Where(snapshot => snapshot.MetricsVersion == filter.MetricsVersion);
            return Task.FromResult<IReadOnlyList<AiTradeValidationMetricSnapshot>>(query.ToArray());
        }
    }

    private sealed class ScoreSnapshotStore(params ContextualScoreSnapshot[] snapshots) : IContextualScoreSnapshotRepository
    {
        public Task<ContextualScoreSnapshot> AddAsync(ContextualScoreSnapshot snapshot, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ContextualScoreSnapshot?> GetByIdAsync(int snapshotId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<ContextualScoreSnapshot>> GetByIdsAsync(string userId, IReadOnlyCollection<int> snapshotIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ContextualScoreSnapshot>>(snapshots.Where(snapshot => snapshot.UserId == userId && snapshotIds.Contains(snapshot.Id)).ToArray());
        public Task<IReadOnlyList<ContextualScoreSnapshot>> GetBySourceAsync(string userId, ContextualScoreSourceKind sourceKind, int? orderId, int? validationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Infrastructure.Order?> GetOrderForUserAsync(int orderId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AiTradeValidation?> GetValidationForUserAsync(int validationId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class LegacyRepository : IAiTradeValidationMetricRepository
    {
        public Task<AiTradeValidation?> GetValidationForMetricAsync(int validationId, string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AiTradeValidationMetric?> GetByValidationIdAsync(int validationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AiTradeValidationMetric?> GetByLinkedOrderIdAsync(int orderId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Infrastructure.Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<AiTradeValidationMetric>> GetForSummaryAsync(string userId, AiValidationMetricFilterDto filter, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(AiTradeValidationMetric metric, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task UpdateAsync(AiTradeValidationMetric metric, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class MetricLog : ILogService
    {
        public void ErrorLog(string methodName, Exception exception) { }
        public void ErrorLog(string methodName, string message, string details) { }
        public void ActivityLog(string userId, string eventType, string description) { }
    }
}
