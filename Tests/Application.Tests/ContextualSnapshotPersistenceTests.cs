using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Application.Tests;

public class ContextualSnapshotPersistenceTests
{
    [Fact]
    public async Task ContextualAndMetricSnapshots_PersistAndRecoverSourceVersionsAndUnknownTotals()
    {
        await using var context = CreateContext();
        await SeedReviewedValidationAsync(context);

        var original = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "contextual-ordinal-legacy",
            ContextualScoreState.NotEvaluable, null, null, null);
        var recalculated = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "contextual-ordinal-v1",
            ContextualScoreState.Complete, 0, 0m, GradeType.C);
        var precise = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "contextual-ordinal-precision",
            ContextualScoreState.Complete, 4, 100m * 4m / 6m, GradeType.BA);
        context.ContextualScoreSnapshots.AddRange(original, recalculated, precise);
        await context.SaveChangesAsync();

        var metric = new AiTradeValidationMetricSnapshot
        {
            PopulationKind = MetricPopulationKind.ReviewedEvaluation,
            UserId = "snapshot-user",
            ValidationId = 17,
            EvaluationRevision = null,
            ContextualScoreSnapshotId = precise.Id,
            StrategyVersion = "strategy-v1",
            EvidenceSchemaVersion = "evidence-v2",
            ClassificationRulesVersion = "contextual-rules-v1",
            ScoreVersion = "contextual-ordinal-v1",
            PromptVersion = "prompt-v3",
            ExtractionSchemaVersion = "provider-schema-v4",
            ProviderName = "Provider-A",
            ModelName = "Vision-B",
            FieldsJson = "{\"fields\":[]}",
            GraphicResultsJson = "{\"verdict\":\"Pending\"}",
            OutcomeObservationsJson = null,
            CapturedAtUtc = DateTimeOffset.Parse("2026-10-23T10:00:00Z")
        };
        context.AiTradeValidationMetricSnapshots.Add(metric);
        await context.SaveChangesAsync();

        var recovered = await context.ContextualScoreSnapshots.AsNoTracking().OrderBy(snapshot => snapshot.Id).ToListAsync();
        var recoveredMetric = await context.AiTradeValidationMetricSnapshots.AsNoTracking().SingleAsync();

        Assert.Equal(3, recovered.Count);
        Assert.Null(recovered[0].EvaluationRevision);
        Assert.Equal("contextual-ordinal-legacy", recovered[0].ScoreVersion);
        Assert.Equal(ContextualScoreState.NotEvaluable, recovered[0].State);
        Assert.Null(recovered[0].ContextRank);
        Assert.Null(recovered[0].Value);
        Assert.Null(recovered[0].Grade);
        Assert.Equal("contextual-ordinal-v1", recovered[1].ScoreVersion);
        Assert.Equal(0, recovered[1].ContextRank);
        Assert.Equal(0m, recovered[1].Value);
        Assert.Equal(GradeType.C, recovered[1].Grade);
        Assert.Equal("contextual-ordinal-precision", recovered[2].ScoreVersion);
        Assert.Equal(4, recovered[2].ContextRank);
        Assert.Equal(100m * 4m / 6m, recovered[2].Value);
        Assert.Equal(GradeType.BA, recovered[2].Grade);
        Assert.Equal(DefinitionJson("contextual-ordinal-v1"), recovered[1].DefinitionJson);
        Assert.Equal("{\"stageId\":null}", recovered[1].InputSnapshotJson);
        Assert.Equal("{\"reasons\":[\"BASE_CONTEXT_REQUIRED\"]}", recovered[1].ComponentsJson);
        Assert.Equal(MetricPopulationKind.ReviewedEvaluation, recoveredMetric.PopulationKind);
        Assert.Null(recoveredMetric.EvaluationRevision);
        Assert.Equal(precise.Id, recoveredMetric.ContextualScoreSnapshotId);
        Assert.Equal("Provider-A", recoveredMetric.ProviderName);
        Assert.Equal("Vision-B", recoveredMetric.ModelName);
        Assert.Equal("{\"fields\":[]}", recoveredMetric.FieldsJson);
        Assert.Equal("{\"verdict\":\"Pending\"}", recoveredMetric.GraphicResultsJson);
        Assert.Null(recoveredMetric.OutcomeObservationsJson);
    }

    [Fact]
    public async Task SnapshotRepositories_SaveAndRecoverOnlyRowsOwnedByTheRequestedUser()
    {
        await using var context = CreateContext();
        await SeedReviewedValidationAsync(context);
        var scoreRepository = new Infrastructure.Persistence.Repositories.ContextualScoreSnapshotRepository(context);
        var metricRepository = new Infrastructure.Persistence.Repositories.AiTradeValidationMetricSnapshotRepository(context);
        var score = await scoreRepository.AddAsync(ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation,
            "contextual-ordinal-v1", ContextualScoreState.Partial, null, null, null), CancellationToken.None);
        var metric = new AiTradeValidationMetricSnapshot
        {
            PopulationKind = MetricPopulationKind.ReviewedEvaluation,
            UserId = "snapshot-user",
            ValidationId = 17,
            ContextualScoreSnapshotId = score.Id,
            FieldsJson = "{\"fields\":[]}",
            CapturedAtUtc = DateTimeOffset.Parse("2026-10-23T10:00:00Z")
        };

        var savedMetric = await metricRepository.AddAsync(metric, CancellationToken.None);

        Assert.Equal(score.Id, (await scoreRepository.GetByIdAsync(score.Id, "snapshot-user", CancellationToken.None))!.Id);
        Assert.Null(await scoreRepository.GetByIdAsync(score.Id, "another-user", CancellationToken.None));
        Assert.Single(await scoreRepository.GetBySourceAsync("snapshot-user", ContextualScoreSourceKind.ReviewedEvaluation,
            null, 17, CancellationToken.None));
        Assert.Equal(savedMetric.Id, (await metricRepository.GetByIdAsync(savedMetric.Id, "snapshot-user", CancellationToken.None))!.Id);
        Assert.Null(await metricRepository.GetByIdAsync(savedMetric.Id, "another-user", CancellationToken.None));
        Assert.Single(await metricRepository.GetForUserAsync("snapshot-user", MetricPopulationKind.ReviewedEvaluation, CancellationToken.None));
    }

    [Fact]
    public async Task SnapshotRepositories_RejectMissingOwnersCrossOwnerSourcesAndMalformedPayloads()
    {
        await using var context = CreateContext();
        await SeedReviewedValidationAsync(context);
        var scoreRepository = new Infrastructure.Persistence.Repositories.ContextualScoreSnapshotRepository(context);
        var metricRepository = new Infrastructure.Persistence.Repositories.AiTradeValidationMetricSnapshotRepository(context);
        var wrongOwner = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "score-v1", ContextualScoreState.Partial, null, null, null);
        wrongOwner.UserId = "another-user";
        var malformed = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "score-v1", ContextualScoreState.Partial, null, null, null);
        malformed.InputSnapshotJson = "not-json";

        await Assert.ThrowsAsync<InvalidOperationException>(() => scoreRepository.AddAsync(wrongOwner, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => scoreRepository.AddAsync(malformed, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => metricRepository.AddAsync(new AiTradeValidationMetricSnapshot
        {
            PopulationKind = MetricPopulationKind.ReviewedEvaluation,
            UserId = "another-user",
            ValidationId = 17,
            FieldsJson = "{\"fields\":[]}",
            CapturedAtUtc = DateTimeOffset.UtcNow
        }, CancellationToken.None));
        Assert.Empty(await context.ContextualScoreSnapshots.ToListAsync());
        Assert.Empty(await context.AiTradeValidationMetricSnapshots.ToListAsync());
    }

    [Fact]
    public async Task MetricSnapshotRepositoryV2_FiltersDimensionsVersionsAndTheAuthenticatedOwner()
    {
        await using var context = CreateContext();
        await SeedReviewedValidationAsync(context);
        var repository = new Infrastructure.Persistence.Repositories.AiTradeValidationMetricSnapshotRepository(context);
        await repository.AddAsync(new AiTradeValidationMetricSnapshot
        {
            PopulationKind = MetricPopulationKind.ReviewedEvaluation,
            UserId = "snapshot-user",
            ValidationId = 17,
            InstrumentId = 3,
            StageId = 2,
            TriggerId = 5,
            ProviderName = "Provider-A",
            ModelName = "Vision-B",
            MetricsVersion = "field-metrics-v2",
            FieldsJson = "{\"fields\":[],\"notApplicableFieldCount\":0}",
            CapturedAtUtc = DateTimeOffset.Parse("2026-10-23T10:00:00Z")
        }, CancellationToken.None);
        var filter = new AiValidationMetricFilterV2Dto(
            PopulationKind: MetricPopulationKind.ReviewedEvaluation,
            ProviderName: "Provider-A",
            ModelName: "Vision-B",
            FromUtc: DateTimeOffset.Parse("2026-10-23T09:00:00Z"),
            ToUtc: DateTimeOffset.Parse("2026-10-23T11:00:00Z"),
            InstrumentId: 3,
            StageId: 2,
            TriggerId: 5,
            MetricsVersion: "field-metrics-v2");

        var owned = await repository.GetForUserAsync("snapshot-user", filter, CancellationToken.None);
        var otherUser = await repository.GetForUserAsync("another-user", filter, CancellationToken.None);
        var noDimensionMatch = await repository.GetForUserAsync("snapshot-user", filter with { StageId = 1 }, CancellationToken.None);

        Assert.Single(owned);
        Assert.Empty(otherUser);
        Assert.Empty(noDimensionMatch);
    }

    [Fact]
    public async Task MetricSnapshotRepositoryV2_RejectsAnInvertedDateRange()
    {
        await using var context = CreateContext();
        var repository = new Infrastructure.Persistence.Repositories.AiTradeValidationMetricSnapshotRepository(context);

        await Assert.ThrowsAsync<ArgumentException>(() => repository.GetForUserAsync("snapshot-user",
            new AiValidationMetricFilterV2Dto(FromUtc: DateTimeOffset.Parse("2026-10-24T00:00:00Z"),
                ToUtc: DateTimeOffset.Parse("2026-10-23T00:00:00Z")), CancellationToken.None));
    }

    [Fact]
    public async Task LegacyMetricSummaryRepository_AlsoScopesRowsThroughTheirValidationOwner()
    {
        await using var context = CreateContext();
        await SeedReviewedValidationAsync(context);
        context.AiTradeValidationMetrics.Add(new Infrastructure.AiTradeValidationMetric
        {
            ValidationId = 17,
            ProviderName = "Provider-A",
            ModelName = "Vision-B",
            HumanCorrectionRate = 0m,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var repository = new Infrastructure.Persistence.Repositories.AiTradeValidationMetricRepository(context, null!);
        var filter = new AiValidationMetricFilterDto { ProviderName = "Provider-A" };

        var owned = await repository.GetForSummaryAsync("snapshot-user", filter, CancellationToken.None);
        var otherUser = await repository.GetForSummaryAsync("another-user", filter, CancellationToken.None);

        Assert.Single(owned);
        Assert.Empty(otherUser);
    }

    [Fact]
    public async Task ScoreRepository_ValidatesCompleteGradeRankDecimalAndTheStoredVersionDefinition()
    {
        await using var context = CreateContext();
        await SeedReviewedValidationAsync(context);
        var repository = new Infrastructure.Persistence.Repositories.ContextualScoreSnapshotRepository(context);
        var definition = DefinitionJson("contextual-ordinal-v1");
        var valid = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "contextual-ordinal-v1",
            ContextualScoreState.Complete, 4, 100m * 4m / 6m, GradeType.BA);
        valid.DefinitionJson = definition;

        var saved = await repository.AddAsync(valid, CancellationToken.None);

        Assert.Equal(GradeType.BA, saved.Grade);
        Assert.Equal(4, saved.ContextRank);
        var mismatch = ContextualSnapshot(ContextualScoreSourceKind.ReviewedEvaluation, "contextual-ordinal-v1",
            ContextualScoreState.Complete, 4, 100m * 4m / 6m, GradeType.B);
        mismatch.DefinitionJson = definition;

        await Assert.ThrowsAsync<ArgumentException>(() => repository.AddAsync(mismatch, CancellationToken.None));
        Assert.Single(await context.ContextualScoreSnapshots.ToListAsync());
    }

    [Fact]
    public void SnapshotMappings_UseVersionedTablesJsonPayloadsDecimalPrecisionAndRestrictiveForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=TradingBookAppModelOnly;User Id=model;Password=model;TrustServerCertificate=True")
            .Options;
        using var context = new ApplicationDbContext(options);
        var designModel = context.GetService<IDesignTimeModel>().Model;
        var score = designModel.FindEntityType(typeof(ContextualScoreSnapshot))!;
        var metric = designModel.FindEntityType(typeof(AiTradeValidationMetricSnapshot))!;

        Assert.Equal("ContextualScoreSnapshots", score.GetTableName());
        Assert.Equal("AiTradeValidationMetricSnapshots", metric.GetTableName());
        Assert.Equal("decimal(30,28)", score.FindProperty(nameof(ContextualScoreSnapshot.Value))!.GetColumnType());
        Assert.Equal("nvarchar(max)", score.FindProperty(nameof(ContextualScoreSnapshot.InputSnapshotJson))!.GetColumnType());
        Assert.Equal("nvarchar(max)", metric.FindProperty(nameof(AiTradeValidationMetricSnapshot.FieldsJson))!.GetColumnType());
        Assert.Contains(score.GetCheckConstraints(), constraint => constraint.Name == "CK_ContextualScoreSnapshots_State");
        Assert.Contains(metric.GetCheckConstraints(), constraint => constraint.Name == "CK_AiTradeValidationMetricSnapshots_Population");
        Assert.All(score.GetForeignKeys(), foreignKey => Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior));
        Assert.All(metric.GetForeignKeys(), foreignKey => Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior));
        Assert.Contains(score.GetIndexes(), index => index.GetDatabaseName() == "IX_ContextualScoreSnapshots_User_CalculatedAtUtc");
        Assert.Contains(metric.GetIndexes(), index => index.GetDatabaseName() == "IX_AiTradeValidationMetricSnapshots_User_Population_CapturedAtUtc");
    }

    private static ContextualScoreSnapshot ContextualSnapshot(ContextualScoreSourceKind source, string version,
        ContextualScoreState state, int? rank, decimal? value, GradeType? grade) => new()
    {
        UserId = "snapshot-user",
        SourceKind = source,
        ValidationId = 17,
        EvaluationRevision = null,
        ScoreVersion = version,
        State = state,
        ContextRank = rank,
        Value = value,
        Grade = grade,
        DefinitionJson = DefinitionJson(version),
        InputSnapshotJson = "{\"stageId\":null}",
        ComponentsJson = "{\"reasons\":[\"BASE_CONTEXT_REQUIRED\"]}",
        CalculatedAtUtc = DateTimeOffset.Parse("2026-10-23T10:00:00Z")
    };

    private static string DefinitionJson(string version) =>
        $$"""{"scoreVersion":"{{version}}","positions":[{"grade":0,"contextRank":6},{"grade":1,"contextRank":5},{"grade":2,"contextRank":4},{"grade":3,"contextRank":3},{"grade":4,"contextRank":2},{"grade":5,"contextRank":1},{"grade":6,"contextRank":0}]}""";

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"snapshot-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedReviewedValidationAsync(ApplicationDbContext context)
    {
        context.Users.Add(new ApplicationUser
        {
            Id = "snapshot-user",
            UserName = "snapshot-user",
            NormalizedUserName = "SNAPSHOT-USER",
            SecurityStamp = "snapshot-stamp"
        });
        context.CatInstruments.Add(new CatInstrument
        {
            Id = 3, Name = "Test", Ticker = "TST", InstrumentType = "Stock", Currency = "USD", Market = "NYSE", IsActived = true
        });
        context.CatDirections.Add(new CatDirection { Id = 2, Code = "LONG" });
        await context.SaveChangesAsync();

        context.AiTradeValidations.Add(new Infrastructure.AiTradeValidation
        {
            Id = 17,
            UserId = "snapshot-user",
            InstrumentId = 3,
            DirectionId = 2,
            EntryPrice = 100m,
            StopLoss = 90m,
            TakeProfit = 120m,
            ValidationStatus = "Pending",
            ProviderName = "Provider-A",
            ModelName = "Vision-B",
            PromptVersion = "prompt-v3",
            SchemaVersion = "provider-schema-v4",
            ModelResponseJson = "{}",
            FinalSummary = "Reviewed source validation",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}
