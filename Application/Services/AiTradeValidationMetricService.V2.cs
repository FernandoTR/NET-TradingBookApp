using System.Text.Json;
using Application.DTOs.AiValidation.V2;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public partial class AiTradeValidationMetricService
{
    private static readonly JsonSerializerOptions MetricSnapshotJsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<AiValidationMetricSummaryV2Dto> GetSummaryV2Async(
        string userId, AiValidationMetricFilterV2Dto filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required for V2 metric queries.", nameof(userId));
        var metricRepository = _metricSnapshotRepositoryV2 ??
            throw new InvalidOperationException("V2 metric snapshot repository is not configured.");
        var scoreRepository = _scoreSnapshotRepository ??
            throw new InvalidOperationException("Contextual score snapshot repository is not configured.");

        cancellationToken.ThrowIfCancellationRequested();
        // Select the source revision before applying provider/version/date filters so filtering cannot
        // resurrect an obsolete revision of a validation or an older recalculation of an order.
        var candidates = await metricRepository.GetForUserAsync(userId,
            new AiValidationMetricFilterV2Dto(PopulationKind: filter.PopulationKind), cancellationToken);
        var selected = ApplyFilters(SelectLatestSources(candidates, filter.IncludeAllRevisions), filter);
        var latestOrders = SelectLatestOrderSources(selected);
        var scoreIds = selected.Concat(latestOrders)
            .Where(snapshot => snapshot.ContextualScoreSnapshotId.HasValue)
            .Select(snapshot => snapshot.ContextualScoreSnapshotId!.Value)
            .Distinct()
            .ToArray();
        var scores = scoreIds.Length == 0
            ? Array.Empty<ContextualScoreSnapshot>()
            : await scoreRepository.GetByIdsAsync(userId, scoreIds, cancellationToken);
        var scoreById = scores.ToDictionary(snapshot => snapshot.Id);

        var groups = selected
            .GroupBy(ToVersionGroup)
            .Select(group => CreateGroup(group.Key, group.ToArray(), scoreById))
            .OrderBy(group => group.Versions.PopulationKind)
            .ThenBy(group => group.Versions.ProviderName, StringComparer.Ordinal)
            .ThenBy(group => group.Versions.ModelName, StringComparer.Ordinal)
            .ThenBy(group => group.Versions.ScoreVersion, StringComparer.Ordinal)
            .ThenBy(group => group.Versions.MetricsVersion, StringComparer.Ordinal)
            .ToArray();

        var outcomes = CreateOutcomeRelations(latestOrders, scoreById);
        return new AiValidationMetricSummaryV2Dto(
            selected.Length,
            selected.Where(snapshot => snapshot.ValidationId.HasValue).Select(snapshot => snapshot.ValidationId!.Value).Distinct().Count(),
            selected.Where(snapshot => snapshot.OrderId.HasValue).Select(snapshot => snapshot.OrderId!.Value).Distinct().Count(),
            groups,
            outcomes,
            selected.Select(snapshot => snapshot.PopulationKind).Distinct().Order().ToArray());
    }

    private static AiTradeValidationMetricSnapshot[] SelectLatestSources(
        IReadOnlyList<AiTradeValidationMetricSnapshot> snapshots, bool includeAllRevisions)
    {
        var selected = snapshots.Where(snapshot => snapshot.PopulationKind == MetricPopulationKind.HistoricalOrder)
            .Where(snapshot => snapshot.OrderId.HasValue)
            .GroupBy(snapshot => snapshot.OrderId!.Value)
            .Select(LatestCaptured)
            .ToList();

        var reviewed = snapshots.Where(snapshot => snapshot.PopulationKind == MetricPopulationKind.ReviewedEvaluation)
            .Where(snapshot => snapshot.ValidationId.HasValue)
            .GroupBy(snapshot => snapshot.ValidationId!.Value);
        foreach (var validationGroup in reviewed)
        {
            // Multiple recalculations of the same source revision are one review row.
            var revisions = validationGroup.GroupBy(snapshot => snapshot.EvaluationRevision)
                .Select(LatestCaptured).ToArray();
            if (includeAllRevisions)
            {
                selected.AddRange(revisions);
                continue;
            }

            // Prefer the greatest known revision. If revisions were never recorded, retain one latest unknown revision.
            var latestKnown = revisions.Where(snapshot => snapshot.EvaluationRevision.HasValue)
                .OrderByDescending(snapshot => snapshot.EvaluationRevision)
                .ThenByDescending(snapshot => snapshot.CapturedAtUtc)
                .ThenByDescending(snapshot => snapshot.Id)
                .FirstOrDefault();
            selected.Add(latestKnown ?? revisions.OrderByDescending(snapshot => snapshot.CapturedAtUtc)
                .ThenByDescending(snapshot => snapshot.Id).First());
        }

        return selected.OrderBy(snapshot => snapshot.CapturedAtUtc).ThenBy(snapshot => snapshot.Id).ToArray();
    }

    private static AiTradeValidationMetricSnapshot[] SelectLatestOrderSources(IReadOnlyList<AiTradeValidationMetricSnapshot> snapshots) =>
        snapshots.Where(snapshot => snapshot.OrderId.HasValue)
            .GroupBy(snapshot => snapshot.OrderId!.Value)
            .Select(LatestCaptured)
            .OrderBy(snapshot => snapshot.OrderId)
            .ToArray();

    private static AiTradeValidationMetricSnapshot LatestCaptured(IEnumerable<AiTradeValidationMetricSnapshot> snapshots) =>
        snapshots.OrderByDescending(snapshot => snapshot.CapturedAtUtc).ThenByDescending(snapshot => snapshot.Id).First();

    private static AiTradeValidationMetricSnapshot[] ApplyFilters(IEnumerable<AiTradeValidationMetricSnapshot> snapshots,
        AiValidationMetricFilterV2Dto filter) => snapshots.Where(snapshot =>
            (string.IsNullOrWhiteSpace(filter.ProviderName) || snapshot.ProviderName == filter.ProviderName.Trim()) &&
            (string.IsNullOrWhiteSpace(filter.ModelName) || snapshot.ModelName == filter.ModelName.Trim()) &&
            (!filter.FromUtc.HasValue || snapshot.CapturedAtUtc >= filter.FromUtc.Value) &&
            (!filter.ToUtc.HasValue || snapshot.CapturedAtUtc <= filter.ToUtc.Value) &&
            (!filter.InstrumentId.HasValue || snapshot.InstrumentId == filter.InstrumentId.Value) &&
            (!filter.StageId.HasValue || snapshot.StageId == filter.StageId.Value) &&
            (!filter.TriggerId.HasValue || snapshot.TriggerId == filter.TriggerId.Value) &&
            (string.IsNullOrWhiteSpace(filter.StrategyVersion) || snapshot.StrategyVersion == filter.StrategyVersion) &&
            (string.IsNullOrWhiteSpace(filter.EvidenceSchemaVersion) || snapshot.EvidenceSchemaVersion == filter.EvidenceSchemaVersion) &&
            (string.IsNullOrWhiteSpace(filter.GraphicalRulesVersion) || snapshot.GraphicalRulesVersion == filter.GraphicalRulesVersion) &&
            (string.IsNullOrWhiteSpace(filter.ClassificationRulesVersion) || snapshot.ClassificationRulesVersion == filter.ClassificationRulesVersion) &&
            (string.IsNullOrWhiteSpace(filter.ScoreVersion) || snapshot.ScoreVersion == filter.ScoreVersion) &&
            (string.IsNullOrWhiteSpace(filter.MetricsVersion) || snapshot.MetricsVersion == filter.MetricsVersion) &&
            (string.IsNullOrWhiteSpace(filter.PromptVersion) || snapshot.PromptVersion == filter.PromptVersion) &&
            (string.IsNullOrWhiteSpace(filter.ExtractionSchemaVersion) || snapshot.ExtractionSchemaVersion == filter.ExtractionSchemaVersion))
        .ToArray();

    private static AiValidationMetricVersionGroupV2Dto ToVersionGroup(AiTradeValidationMetricSnapshot snapshot) => new(
        snapshot.PopulationKind, snapshot.ProviderName, snapshot.ModelName, snapshot.StrategyVersion,
        snapshot.EvidenceSchemaVersion, snapshot.GraphicalRulesVersion, snapshot.ClassificationRulesVersion,
        snapshot.ScoreVersion, snapshot.MetricsVersion, snapshot.PromptVersion, snapshot.ExtractionSchemaVersion);

    private static AiValidationMetricGroupV2Dto CreateGroup(AiValidationMetricVersionGroupV2Dto versions,
        IReadOnlyList<AiTradeValidationMetricSnapshot> snapshots, IReadOnlyDictionary<int, ContextualScoreSnapshot> scores)
    {
        // Historical orders do not contain AI-to-user review data by default; only reviewed validation rows
        // contribute observations to field coverage/concordance denominators.
        var fieldSnapshots = snapshots.Where(snapshot => snapshot.PopulationKind == MetricPopulationKind.ReviewedEvaluation)
            .Select(snapshot => ReadFields(snapshot.FieldsJson)).ToArray();
        var fieldMetrics = SummarizeFields(versions.MetricsVersion, fieldSnapshots);
        var scenarioPairs = snapshots
            .Select(snapshot => (Snapshot: snapshot, Graphical: ReadGraphical(snapshot.GraphicResultsJson)))
            .Select(row => (row.Snapshot, row.Graphical, Score: FindRelatedScore(row.Snapshot, scores)))
            .Where(row => row.Graphical?.ManualScenarioCode is not null &&
                row.Score is { State: ContextualScoreState.Complete, Grade: not null })
            .Select(row => (Scenario: NormalizeGrade(row.Graphical!.ManualScenarioCode), Grade: row.Score!.Grade))
            .Where(pair => pair.Scenario.HasValue)
            .ToArray();
        var identifiedVerdicts = snapshots.Select(snapshot => ReadGraphical(snapshot.GraphicResultsJson)?.Evaluation?.Verdict)
            .Where(verdict => verdict.HasValue && Enum.IsDefined(verdict.Value))
            .Select(verdict => verdict!.Value)
            .ToArray();
        var verdictDistribution = Enum.GetValues<SetupVerdict>().OrderBy(verdict => verdict)
            .Select(verdict => new AiValidationMetricVerdictDistributionV2Dto(
                verdict, identifiedVerdicts.Count(item => item == verdict), identifiedVerdicts.Length,
                Rate(identifiedVerdicts.Count(item => item == verdict), identifiedVerdicts.Length).Percentage))
            .ToArray();

        return new AiValidationMetricGroupV2Dto(
            versions,
            snapshots.Count,
            fieldMetrics,
            Rate(scenarioPairs.Count(pair => pair.Scenario == pair.Grade), scenarioPairs.Length),
            verdictDistribution,
            fieldSnapshots.Sum(fieldSnapshot => fieldSnapshot.NotApplicableFieldCount),
            fieldMetrics.NoReviewCount);
    }

    private static AiValidationMetricFieldsSnapshotDto ReadFields(string json)
    {
        try
        {
            var value = JsonSerializer.Deserialize<AiValidationMetricFieldsSnapshotDto>(json, MetricSnapshotJsonOptions);
            if (value?.Fields is null || value.NotApplicableFieldCount < 0)
                throw new InvalidOperationException("METRIC_FIELD_SNAPSHOT_INVALID");
            return value;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("METRIC_FIELD_SNAPSHOT_INVALID", exception);
        }
    }

    private static AiValidationMetricGraphicalSnapshotDto? ReadGraphical(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<AiValidationMetricGraphicalSnapshotDto>(json, MetricSnapshotJsonOptions); }
        catch (JsonException exception) { throw new InvalidOperationException("METRIC_GRAPHICAL_SNAPSHOT_INVALID", exception); }
    }

    private static AiValidationOutcomeObservationDto? ReadOutcome(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<AiValidationOutcomeObservationDto>(json, MetricSnapshotJsonOptions); }
        catch (JsonException exception) { throw new InvalidOperationException("METRIC_OUTCOME_SNAPSHOT_INVALID", exception); }
    }

    private static AiValidationFieldMetricMeasurementV2Dto SummarizeFields(string? metricsVersion,
        IReadOnlyList<AiValidationMetricFieldsSnapshotDto> snapshots)
    {
        var fields = snapshots.SelectMany(snapshot => snapshot.Fields).ToArray();
        var reviewed = fields.Where(field => field.ChangeKind != AiValidationFieldChangeKind.NoReview).ToArray();
        var knownAiAssertions = reviewed.Where(field => IsKnown(field.AiAvailability)).ToArray();
        var unknownAiReviewed = reviewed.Where(field => field.AiAvailability == EvidenceAvailability.Unknown).ToArray();
        var comparable = fields.Where(field => field.Matches.HasValue).ToArray();
        return new AiValidationFieldMetricMeasurementV2Dto(
            metricsVersion,
            Rate(fields.Count(field => IsKnown(field.AiAvailability)), fields.Length),
            Rate(comparable.Count(field => field.Matches == true), comparable.Length),
            Rate(knownAiAssertions.Count(field => field.ChangeKind is AiValidationFieldChangeKind.Correction or AiValidationFieldChangeKind.Discarded),
                knownAiAssertions.Length),
            Rate(unknownAiReviewed.Count(field => field.ChangeKind == AiValidationFieldChangeKind.ManualCompletion), unknownAiReviewed.Length),
            Rate(reviewed.Count(field => field.ChangeKind == AiValidationFieldChangeKind.Discarded), reviewed.Length),
            fields,
            snapshots.Sum(snapshot => snapshot.NotApplicableFieldCount),
            fields.Count(field => field.ChangeKind == AiValidationFieldChangeKind.NoReview));
    }

    private static IReadOnlyList<AiValidationMetricOutcomeRelationV2Dto> CreateOutcomeRelations(
        IReadOnlyList<AiTradeValidationMetricSnapshot> latestOrders, IReadOnlyDictionary<int, ContextualScoreSnapshot> scores)
    {
        var outcomeRows = new List<OutcomeRow>();
        foreach (var snapshot in latestOrders)
        {
            if (!snapshot.OrderId.HasValue || !snapshot.ContextualScoreSnapshotId.HasValue ||
                !scores.TryGetValue(snapshot.ContextualScoreSnapshotId.Value, out var score) || !MatchesSource(snapshot, score))
                continue;

            var observation = ReadOutcome(snapshot.OutcomeObservationsJson);
            if (observation is null || !HasOutcome(observation)) continue;
            outcomeRows.Add(new(snapshot.OrderId.Value, snapshot.PopulationKind, score, observation));
        }

        return outcomeRows.GroupBy(row => new OutcomeGroupKey(row.PopulationKind, row.Score.ScoreVersion,
                row.Score.ContextRank, row.Score.Grade, row.Score.Value))
            .OrderBy(group => group.Key.PopulationKind)
            .ThenBy(group => group.Key.ScoreVersion, StringComparer.Ordinal)
            .ThenBy(group => group.Key.ContextRank)
            .ThenBy(group => group.Key.Grade)
            .Select(group => new AiValidationMetricOutcomeRelationV2Dto(
                group.Key.PopulationKind,
                group.Key.ScoreVersion,
                group.Key.ContextRank,
                group.Key.Grade,
                group.Key.ScoreValue,
                group.Select(row => row.OrderId).Distinct().Count(),
                group.Select(row => row.Observation.OutcomeCode).Where(code => !string.IsNullOrWhiteSpace(code))
                    .Select(code => code!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                MinObservedAt(group),
                MaxObservedAt(group),
                Rate(group.Count(row => row.Observation.ReachedSl == true), group.Count(row => row.Observation.ReachedSl.HasValue)),
                Rate(group.Count(row => row.Observation.ReachedTp1 == true), group.Count(row => row.Observation.ReachedTp1.HasValue)),
                Rate(group.Count(row => row.Observation.ReachedTp2 == true), group.Count(row => row.Observation.ReachedTp2.HasValue)),
                Rate(group.Count(row => row.Observation.ReachedTp3 == true), group.Count(row => row.Observation.ReachedTp3.HasValue))))
            .ToArray();
    }

    private static DateTimeOffset? MinObservedAt(IEnumerable<OutcomeRow> rows)
    {
        var dates = rows.Where(row => row.Observation.ObservedAtUtc.HasValue)
            .Select(row => row.Observation.ObservedAtUtc!.Value).ToArray();
        return dates.Length == 0 ? null : dates.Min();
    }

    private static DateTimeOffset? MaxObservedAt(IEnumerable<OutcomeRow> rows)
    {
        var dates = rows.Where(row => row.Observation.ObservedAtUtc.HasValue)
            .Select(row => row.Observation.ObservedAtUtc!.Value).ToArray();
        return dates.Length == 0 ? null : dates.Max();
    }

    private static ContextualScoreSnapshot? FindRelatedScore(AiTradeValidationMetricSnapshot snapshot,
        IReadOnlyDictionary<int, ContextualScoreSnapshot> scores) =>
        snapshot.ContextualScoreSnapshotId.HasValue && scores.TryGetValue(snapshot.ContextualScoreSnapshotId.Value, out var score) &&
        MatchesSource(snapshot, score) ? score : null;

    private static bool MatchesSource(AiTradeValidationMetricSnapshot metric, ContextualScoreSnapshot score)
    {
        if (metric.UserId != score.UserId || metric.PopulationKind == MetricPopulationKind.HistoricalOrder &&
            (score.SourceKind != ContextualScoreSourceKind.HistoricalOrder || score.OrderId != metric.OrderId)) return false;
        if (metric.PopulationKind == MetricPopulationKind.ReviewedEvaluation &&
            (score.SourceKind != ContextualScoreSourceKind.ReviewedEvaluation || score.ValidationId != metric.ValidationId ||
             score.EvaluationRevision != metric.EvaluationRevision)) return false;
        return metric.ScoreVersion is null || metric.ScoreVersion == score.ScoreVersion;
    }

    private static GradeType? NormalizeGrade(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var normalized = string.Concat(code.Where(char.IsLetterOrDigit)).ToUpperInvariant();
        return Enum.TryParse<GradeType>(normalized, ignoreCase: false, out var grade) && Enum.IsDefined(grade) ? grade : null;
    }

    private static bool IsKnown(EvidenceAvailability availability) =>
        availability is EvidenceAvailability.Present or EvidenceAvailability.Absent;

    private static bool HasOutcome(AiValidationOutcomeObservationDto outcome) => outcome.ReachedSl.HasValue || outcome.ReachedTp1.HasValue ||
        outcome.ReachedTp2.HasValue || outcome.ReachedTp3.HasValue || !string.IsNullOrWhiteSpace(outcome.OutcomeCode);

    private static RateMetricDto Rate(int numerator, int denominator) => new(
        numerator, denominator,
        denominator == 0 ? null : Math.Round(numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero));

    private sealed record OutcomeRow(int OrderId, MetricPopulationKind PopulationKind, ContextualScoreSnapshot Score,
        AiValidationOutcomeObservationDto Observation);
    private sealed record OutcomeGroupKey(MetricPopulationKind PopulationKind, string ScoreVersion, int? ContextRank,
        GradeType? Grade, decimal? ScoreValue);
}
