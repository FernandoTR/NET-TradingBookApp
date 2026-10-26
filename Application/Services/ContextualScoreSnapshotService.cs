using System.Text.Json;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>Recalculates selected user-owned sources and appends immutable, versioned results without replacing originals.</summary>
/// <param name="repository">Source-scoped snapshot and source-row access.</param>
/// <param name="metricSnapshotRepository">Snapshot persistence and source ownership checks for metric reviews.</param>
/// <param name="inputAdapter">Conservative adaptation of recorded historical and reviewed values.</param>
/// <param name="scoreEngine">The one-evaluation contextual classifier and ordinal score engine.</param>
/// <param name="logService">Logging for per-source failures without disclosing data in result error codes.</param>
public sealed class ContextualScoreSnapshotService(
    IContextualScoreSnapshotRepository repository,
    IAiTradeValidationMetricSnapshotRepository metricSnapshotRepository,
    IContextualScoreInputAdapter inputAdapter,
    ITradingScoreEngineService scoreEngine,
    ILogService logService) : IContextualScoreSnapshotService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IContextualScoreSnapshotRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IAiTradeValidationMetricSnapshotRepository _metricSnapshotRepository =
        metricSnapshotRepository ?? throw new ArgumentNullException(nameof(metricSnapshotRepository));
    private readonly IContextualScoreInputAdapter _inputAdapter = inputAdapter ?? throw new ArgumentNullException(nameof(inputAdapter));
    private readonly ITradingScoreEngineService _scoreEngine = scoreEngine ?? throw new ArgumentNullException(nameof(scoreEngine));
    private readonly ILogService _logService = logService ?? throw new ArgumentNullException(nameof(logService));

    /// <inheritdoc />
    public async Task<ContextualScoreRecalculationBatchDto> RecalculateHistoricalOrdersAsync(
        string userId, IReadOnlyCollection<int> orderIds, CancellationToken cancellationToken)
    {
        ValidateRequest(userId, orderIds);
        var ids = orderIds.Distinct().ToArray();
        var results = new List<ContextualScoreRecalculationItemDto>(ids.Length);
        foreach (var orderId in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (orderId <= 0)
            {
                results.Add(Failure(ContextualScoreSourceKind.HistoricalOrder, orderId, null, null, "ORDER_ID_INVALID"));
                continue;
            }

            try
            {
                var order = await _repository.GetOrderForUserAsync(orderId, userId, cancellationToken);
                if (order is null)
                {
                    results.Add(Failure(ContextualScoreSourceKind.HistoricalOrder, orderId, null, null, "ORDER_NOT_FOUND_FOR_USER"));
                    continue;
                }

                var input = await _inputAdapter.FromOrderAsync(order, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                var evaluation = _scoreEngine.Evaluate(input);
                var snapshot = await _repository.AddAsync(CreateSnapshot(userId, ContextualScoreSourceKind.HistoricalOrder,
                    order.Id, null, null, evaluation), cancellationToken);
                results.Add(Success(snapshot));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logService.ErrorLog(nameof(RecalculateHistoricalOrdersAsync), exception);
                results.Add(Failure(ContextualScoreSourceKind.HistoricalOrder, orderId, null, null, "ORDER_RECALCULATION_FAILED"));
            }
        }

        return new(ContextualScoreSourceKind.HistoricalOrder, ids.Length, results);
    }

    /// <inheritdoc />
    public async Task<ContextualScoreRecalculationBatchDto> RecalculateReviewedEvaluationsAsync(
        string userId, IReadOnlyCollection<ReviewedEvaluationSnapshotRequestDto> evaluations, CancellationToken cancellationToken)
    {
        ValidateRequest(userId, evaluations);
        var requests = evaluations.DistinctBy(request => (request.ValidationId, request.EvaluationRevision)).ToArray();
        var results = new List<ContextualScoreRecalculationItemDto>(requests.Length);
        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.ValidationId <= 0 || request.EvaluationRevision is <= 0)
            {
                results.Add(Failure(ContextualScoreSourceKind.ReviewedEvaluation, null, request.ValidationId,
                    request.EvaluationRevision, "VALIDATION_SOURCE_INVALID"));
                continue;
            }

            try
            {
                var validation = await _repository.GetValidationForUserAsync(request.ValidationId, userId, cancellationToken);
                if (validation is null)
                {
                    results.Add(Failure(ContextualScoreSourceKind.ReviewedEvaluation, null, request.ValidationId,
                        request.EvaluationRevision, "VALIDATION_NOT_FOUND_FOR_USER"));
                    continue;
                }

                if (!validation.ConfirmedAt.HasValue)
                {
                    results.Add(Failure(ContextualScoreSourceKind.ReviewedEvaluation, validation.OrderId, validation.Id,
                        request.EvaluationRevision, "VALIDATION_NOT_REVIEWED"));
                    continue;
                }

                var input = await _inputAdapter.FromValidationAsync(validation, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                var evaluation = _scoreEngine.Evaluate(input);
                var snapshot = await _repository.AddAsync(CreateSnapshot(userId, ContextualScoreSourceKind.ReviewedEvaluation,
                    validation.OrderId, validation.Id, request.EvaluationRevision, evaluation), cancellationToken);
                results.Add(Success(snapshot));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logService.ErrorLog(nameof(RecalculateReviewedEvaluationsAsync), exception);
                results.Add(Failure(ContextualScoreSourceKind.ReviewedEvaluation, null, request.ValidationId,
                    request.EvaluationRevision, "VALIDATION_RECALCULATION_FAILED"));
            }
        }

        return new(ContextualScoreSourceKind.ReviewedEvaluation, requests.Length, results);
    }

    /// <summary>Persists one metric snapshot after confirming the source row and any linked score share this owner.</summary>
    /// <param name="snapshot">Metric evidence with its explicit population, versions and optional score-snapshot reference.</param>
    /// <param name="cancellationToken">Cancellation for ownership checks and persistence.</param>
    /// <returns>The saved immutable metric snapshot.</returns>
    public Task<AiTradeValidationMetricSnapshot> SaveMetricSnapshotAsync(
        AiTradeValidationMetricSnapshot snapshot, CancellationToken cancellationToken) =>
        _metricSnapshotRepository.AddAsync(snapshot, cancellationToken);

    private static ContextualScoreSnapshot CreateSnapshot(string userId, ContextualScoreSourceKind sourceKind,
        int? orderId, int? validationId, int? revision, ContextualScoreEvaluationDto evaluation) => new()
    {
        UserId = userId,
        SourceKind = sourceKind,
        OrderId = orderId,
        ValidationId = validationId,
        EvaluationRevision = revision,
        ScoreVersion = evaluation.Score.ScoreVersion,
        State = evaluation.Score.State,
        ContextRank = evaluation.Score.ContextRank,
        Value = evaluation.Score.Value,
        Grade = evaluation.Score.Grade,
        DefinitionJson = JsonSerializer.Serialize(evaluation.Definition, JsonOptions),
        InputSnapshotJson = JsonSerializer.Serialize(evaluation.Input, JsonOptions),
        ComponentsJson = JsonSerializer.Serialize(new ContextualScoreSnapshotBreakdownDto(evaluation.Classification, evaluation.Score), JsonOptions),
        CalculatedAtUtc = DateTimeOffset.UtcNow
    };

    private static ContextualScoreRecalculationItemDto Success(ContextualScoreSnapshot snapshot) => new(
        snapshot.SourceKind, snapshot.OrderId, snapshot.ValidationId, snapshot.EvaluationRevision, true,
        snapshot.Id, snapshot.State, null);

    private static ContextualScoreRecalculationItemDto Failure(ContextualScoreSourceKind sourceKind, int? orderId,
        int? validationId, int? revision, string code) => new(sourceKind, orderId, validationId, revision, false, null, null, code);

    private static void ValidateRequest(string userId, object? sources)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("A source owner is required.", nameof(userId));
        ArgumentNullException.ThrowIfNull(sources);
    }

    private sealed record ContextualScoreSnapshotBreakdownDto(
        ScenarioGradeResultDto Classification,
        ContextualScoreResultDto Score);
}
