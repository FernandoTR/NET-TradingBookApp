using Application.DTOs.AiValidation;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AiTradeValidationMetricService : IAiTradeValidationMetricService
{
    private const int ClosedOrderStatusId = 2;
    private const string OutcomeSl = "SL";
    private const string OutcomeTp1 = "TP1";
    private const string OutcomeTp2 = "TP2";
    private const string OutcomeTp3 = "TP3";
    private const string OutcomeOpen = "Open";
    private const string OutcomeUnknown = "Unknown";

    private readonly IAiTradeValidationMetricRepository _metricRepository;
    private readonly ILogService _logService;

    public AiTradeValidationMetricService(
        IAiTradeValidationMetricRepository metricRepository,
        ILogService logService)
    {
        _metricRepository = metricRepository;
        _logService = logService;
    }

    public async Task CreateInitialMetricAsync(int validationId, string userId, CancellationToken cancellationToken)
    {
        if (validationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(validationId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId is required to create an AI validation metric.", nameof(userId));
        }

        try
        {
            var validation = await _metricRepository.GetValidationForMetricAsync(validationId, userId, cancellationToken);
            if (validation is null)
            {
                return;
            }

            var metric = CreateInitialMetric(validation);
            var existingMetric = await _metricRepository.GetByValidationIdAsync(validationId, cancellationToken);

            if (existingMetric is null)
            {
                await _metricRepository.AddAsync(metric, cancellationToken);
                return;
            }

            ApplyInitialMetric(existingMetric, metric);
            await _metricRepository.UpdateAsync(existingMetric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(CreateInitialMetricAsync), ex);
            throw;
        }
    }

    public async Task RefreshOrderOutcomeAsync(int orderId, CancellationToken cancellationToken)
    {
        if (orderId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        try
        {
            var metric = await _metricRepository.GetByLinkedOrderIdAsync(orderId, cancellationToken);
            if (metric is null)
            {
                return;
            }

            var order = await _metricRepository.GetOrderByIdAsync(orderId, cancellationToken);
            if (order is null)
            {
                return;
            }

            metric.OrderId = order.Id;
            metric.ReachedSl = order.Sl;
            metric.ReachedTp1 = order.Tp1;
            metric.ReachedTp2 = order.Tp2;
            metric.ReachedTp3 = order.Tp3;
            metric.OutcomeClassification = ClassifyOutcome(order);
            metric.UpdatedAt = DateTime.UtcNow;

            await _metricRepository.UpdateAsync(metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(RefreshOrderOutcomeAsync), ex);
            throw;
        }
    }

    public async Task<AiValidationMetricSummaryDto> GetSummaryAsync(AiValidationMetricFilterDto filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var metrics = await _metricRepository.GetForSummaryAsync(filter, cancellationToken);
        var groups = metrics
            .GroupBy(metric => new { metric.ProviderName, metric.ModelName })
            .OrderBy(group => group.Key.ProviderName)
            .ThenBy(group => group.Key.ModelName)
            .Select(CreateSummaryGroup)
            .ToList();

        return new AiValidationMetricSummaryDto
        {
            SampleSize = metrics.Count,
            AverageHumanCorrectionRate = CalculateAverageHumanCorrectionRate(metrics),
            Groups = groups,
            Metrics = metrics.Select(MapMetric).ToList()
        };
    }

    private static AiTradeValidationMetric CreateInitialMetric(AiTradeValidation validation)
    {
        var triggerMatchedUser = Match(validation.DetectedTriggerId, validation.ConfirmedTriggerId);
        var sceneryMatchedUser = Match(validation.DetectedSceneryId, validation.ConfirmedSceneryId);
        var stageMatchedUser = Match(validation.DetectedStageId, validation.ConfirmedStageId);
        var figureMatchedUser = Match(validation.DetectedFigureId, validation.ConfirmedFigureId);
        var frameMatchedUser = Match(validation.DetectedFrameId, validation.ConfirmedFrameId);
        bool? directionMatchedUser = null;
        var trendMatchedUser = Match(validation.DetectedIsTrendAligned, validation.ConfirmedIsTrendAligned);
        var locationMatchedUser = Match(validation.DetectedLocationType, validation.ConfirmedLocationType);
        var confirmationMatchedUser = Match(validation.DetectedConfirmationType, validation.ConfirmedConfirmationType);
        var pivotZoneMatchedUser = Match(validation.DetectedIsPivotZone, validation.ConfirmedIsPivotZone);

        return new AiTradeValidationMetric
        {
            ValidationId = validation.Id,
            OrderId = validation.OrderId,
            ProviderName = validation.ProviderName,
            ModelName = validation.ModelName,
            TriggerMatchedUser = triggerMatchedUser,
            SceneryMatchedUser = sceneryMatchedUser,
            StageMatchedUser = stageMatchedUser,
            FigureMatchedUser = figureMatchedUser,
            FrameMatchedUser = frameMatchedUser,
            DirectionMatchedUser = directionMatchedUser,
            TrendMatchedUser = trendMatchedUser,
            LocationMatchedUser = locationMatchedUser,
            ConfirmationMatchedUser = confirmationMatchedUser,
            PivotZoneMatchedUser = pivotZoneMatchedUser,
            HumanCorrectionRate = CalculateHumanCorrectionRate(
                triggerMatchedUser,
                sceneryMatchedUser,
                stageMatchedUser,
                figureMatchedUser,
                frameMatchedUser,
                directionMatchedUser,
                trendMatchedUser,
                locationMatchedUser,
                confirmationMatchedUser,
                pivotZoneMatchedUser),
            TotalScore = validation.TotalScore,
            Grade = validation.Grade,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ApplyInitialMetric(AiTradeValidationMetric target, AiTradeValidationMetric source)
    {
        target.OrderId = source.OrderId;
        target.ProviderName = source.ProviderName;
        target.ModelName = source.ModelName;
        target.TriggerMatchedUser = source.TriggerMatchedUser;
        target.SceneryMatchedUser = source.SceneryMatchedUser;
        target.StageMatchedUser = source.StageMatchedUser;
        target.FigureMatchedUser = source.FigureMatchedUser;
        target.FrameMatchedUser = source.FrameMatchedUser;
        target.DirectionMatchedUser = source.DirectionMatchedUser;
        target.TrendMatchedUser = source.TrendMatchedUser;
        target.LocationMatchedUser = source.LocationMatchedUser;
        target.ConfirmationMatchedUser = source.ConfirmationMatchedUser;
        target.PivotZoneMatchedUser = source.PivotZoneMatchedUser;
        target.HumanCorrectionRate = source.HumanCorrectionRate;
        target.TotalScore = source.TotalScore;
        target.Grade = source.Grade;
        target.UpdatedAt = DateTime.UtcNow;
    }

    private static bool? Match<TValue>(TValue? detected, TValue? confirmed)
        where TValue : struct
    {
        if (!detected.HasValue || !confirmed.HasValue)
        {
            return null;
        }

        return EqualityComparer<TValue>.Default.Equals(detected.Value, confirmed.Value);
    }

    private static decimal CalculateHumanCorrectionRate(params bool?[] matches)
    {
        var availableMatches = matches.Where(match => match.HasValue).ToList();
        if (availableMatches.Count == 0)
        {
            return 0m;
        }

        var correctedFields = availableMatches.Count(match => match == false);
        return Math.Round(correctedFields * 100m / availableMatches.Count, 2, MidpointRounding.AwayFromZero);
    }

    private static AiValidationMetricSummaryGroupDto CreateSummaryGroup(IGrouping<object, AiTradeValidationMetric> group)
    {
        var metrics = group.ToList();
        var first = metrics[0];

        return new AiValidationMetricSummaryGroupDto
        {
            ProviderName = first.ProviderName,
            ModelName = first.ModelName,
            SampleSize = metrics.Count,
            TriggerPrecision = CalculatePrecision(metrics, metric => metric.TriggerMatchedUser),
            SceneryPrecision = CalculatePrecision(metrics, metric => metric.SceneryMatchedUser),
            StagePrecision = CalculatePrecision(metrics, metric => metric.StageMatchedUser),
            FigurePrecision = CalculatePrecision(metrics, metric => metric.FigureMatchedUser),
            FramePrecision = CalculatePrecision(metrics, metric => metric.FrameMatchedUser),
            DirectionPrecision = CalculatePrecision(metrics, metric => metric.DirectionMatchedUser),
            TrendPrecision = CalculatePrecision(metrics, metric => metric.TrendMatchedUser),
            LocationPrecision = CalculatePrecision(metrics, metric => metric.LocationMatchedUser),
            ConfirmationPrecision = CalculatePrecision(metrics, metric => metric.ConfirmationMatchedUser),
            PivotZonePrecision = CalculatePrecision(metrics, metric => metric.PivotZoneMatchedUser),
            AverageHumanCorrectionRate = CalculateAverageHumanCorrectionRate(metrics),
            GradeOutcomes = CreateGradeOutcomeRows(metrics)
        };
    }

    private static decimal? CalculatePrecision(
        IReadOnlyCollection<AiTradeValidationMetric> metrics,
        Func<AiTradeValidationMetric, bool?> selector)
    {
        var availableMatches = metrics
            .Select(selector)
            .Where(match => match.HasValue)
            .ToList();

        if (availableMatches.Count == 0)
        {
            return null;
        }

        var matchedFields = availableMatches.Count(match => match == true);
        return Math.Round(matchedFields * 100m / availableMatches.Count, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateAverageHumanCorrectionRate(IReadOnlyCollection<AiTradeValidationMetric> metrics)
    {
        return metrics.Count == 0
            ? 0m
            : Math.Round(metrics.Average(metric => metric.HumanCorrectionRate), 2, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<AiValidationMetricGradeOutcomeDto> CreateGradeOutcomeRows(
        IReadOnlyCollection<AiTradeValidationMetric> metrics)
    {
        var metricsWithOutcome = metrics
            .Where(metric => !string.IsNullOrWhiteSpace(metric.OutcomeClassification))
            .ToList();

        if (metricsWithOutcome.Count == 0)
        {
            return [];
        }

        return metricsWithOutcome
            .GroupBy(metric => new { metric.Grade, metric.OutcomeClassification })
            .OrderBy(group => group.Key.Grade)
            .ThenBy(group => group.Key.OutcomeClassification)
            .Select(group => new AiValidationMetricGradeOutcomeDto
            {
                Grade = group.Key.Grade,
                OutcomeClassification = group.Key.OutcomeClassification!,
                Count = group.Count(),
                Percentage = Math.Round(group.Count() * 100m / metricsWithOutcome.Count, 2, MidpointRounding.AwayFromZero)
            })
            .ToList();
    }

    private static AiValidationMetricDto MapMetric(AiTradeValidationMetric metric)
    {
        return new AiValidationMetricDto
        {
            Id = metric.Id,
            ValidationId = metric.ValidationId,
            OrderId = metric.OrderId,
            ProviderName = metric.ProviderName,
            ModelName = metric.ModelName,
            TriggerMatchedUser = metric.TriggerMatchedUser,
            SceneryMatchedUser = metric.SceneryMatchedUser,
            StageMatchedUser = metric.StageMatchedUser,
            FigureMatchedUser = metric.FigureMatchedUser,
            FrameMatchedUser = metric.FrameMatchedUser,
            DirectionMatchedUser = metric.DirectionMatchedUser,
            TrendMatchedUser = metric.TrendMatchedUser,
            LocationMatchedUser = metric.LocationMatchedUser,
            ConfirmationMatchedUser = metric.ConfirmationMatchedUser,
            PivotZoneMatchedUser = metric.PivotZoneMatchedUser,
            HumanCorrectionRate = metric.HumanCorrectionRate,
            TotalScore = metric.TotalScore,
            Grade = metric.Grade,
            ReachedSl = metric.ReachedSl,
            ReachedTp1 = metric.ReachedTp1,
            ReachedTp2 = metric.ReachedTp2,
            ReachedTp3 = metric.ReachedTp3,
            OutcomeClassification = metric.OutcomeClassification,
            CreatedAt = metric.CreatedAt,
            UpdatedAt = metric.UpdatedAt
        };
    }

    private static string ClassifyOutcome(Order order)
    {
        if (order.CatStatusId != ClosedOrderStatusId)
        {
            return OutcomeOpen;
        }

        if (order.Tp3 == true)
        {
            return OutcomeTp3;
        }

        if (order.Tp2 == true)
        {
            return OutcomeTp2;
        }

        if (order.Tp1 == true)
        {
            return OutcomeTp1;
        }

        return order.Sl == true ? OutcomeSl : OutcomeUnknown;
    }
}
