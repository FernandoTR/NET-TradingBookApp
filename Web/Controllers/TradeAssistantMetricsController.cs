using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[Authorize]
public class TradeAssistantMetricsController : Controller
{
    private readonly IAiTradeValidationMetricService _metricService;
    private readonly ILogService _logService;
    private readonly IIdentityService _identityService;

    public TradeAssistantMetricsController(
        IAiTradeValidationMetricService metricService,
        ILogService logService,
        IIdentityService identityService)
    {
        _metricService = metricService;
        _logService = logService;
        _identityService = identityService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? providerName,
        string? modelName,
        DateTime? from,
        DateTime? to,
        string? populationKind,
        int? instrumentId,
        int? stageId,
        int? triggerId,
        string? strategyVersion,
        string? evidenceSchemaVersion,
        string? graphicalRulesVersion,
        string? classificationRulesVersion,
        string? scoreVersion,
        string? metricsVersion,
        string? promptVersion,
        string? extractionSchemaVersion,
        bool includeAllRevisions,
        CancellationToken cancellationToken)
    {
        var population = ParsePopulation(populationKind);
        var filter = new AiValidationMetricFilterV2Dto(
            PopulationKind: population,
            ProviderName: Clean(providerName),
            ModelName: Clean(modelName),
            FromUtc: DateStartUtc(from),
            ToUtc: DateEndUtc(to),
            InstrumentId: PositiveOrNull(instrumentId),
            StageId: PositiveOrNull(stageId),
            TriggerId: PositiveOrNull(triggerId),
            StrategyVersion: Clean(strategyVersion),
            EvidenceSchemaVersion: Clean(evidenceSchemaVersion),
            GraphicalRulesVersion: Clean(graphicalRulesVersion),
            ClassificationRulesVersion: Clean(classificationRulesVersion),
            ScoreVersion: Clean(scoreVersion),
            MetricsVersion: Clean(metricsVersion),
            PromptVersion: Clean(promptVersion),
            ExtractionSchemaVersion: Clean(extractionSchemaVersion),
            IncludeAllRevisions: includeAllRevisions);

        var viewModel = new TradeAssistantMetricsViewModel
        {
            Filter = filter
        };

        if ((!string.IsNullOrWhiteSpace(populationKind) && !population.HasValue) ||
            new[] { instrumentId, stageId, triggerId }.Any(id => id is <= 0) ||
            (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date))
        {
            viewModel.IsV2Available = false;
            viewModel.V2UnavailableMessage = "Revisa los filtros: el rango de fechas, la poblacion o alguna dimension no es valida.";
            TempData["TradeAssistantMetricsMessage"] = viewModel.V2UnavailableMessage;
            return View(viewModel);
        }

        var userId = _identityService.GetCurrentUserId();
        var legacyFilter = new AiValidationMetricFilterDto
        {
            ProviderName = filter.ProviderName,
            ModelName = filter.ModelName,
            From = from?.Date,
            To = to?.Date.AddDays(1).AddTicks(-1)
        };

        try
        {
            viewModel.LegacySummary = await _metricService.GetSummaryAsync(userId, legacyFilter, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: TradeAssistantMetrics, Action: {nameof(Index)}", ex);
            TempData["TradeAssistantMetricsMessage"] = "No fue posible cargar las metricas del modelo anterior.";
        }

        try
        {
            viewModel.SummaryV2 = await _metricService.GetSummaryV2Async(userId, filter, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: TradeAssistantMetrics, Action: {nameof(Index)} V2", ex);
            viewModel.V2UnavailableMessage = "Las instantaneas V2 no estan disponibles para esta consulta.";
            viewModel.IsV2Available = false;
        }

        return View(viewModel);
    }

    private static MetricPopulationKind? ParsePopulation(string? value)
    {
        return Enum.TryParse<MetricPopulationKind>(value, ignoreCase: true, out var population) && Enum.IsDefined(population)
            ? population
            : null;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? PositiveOrNull(int? value) => value is > 0 ? value : null;

    private static DateTimeOffset? DateStartUtc(DateTime? value)
    {
        return value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc))
            : null;
    }

    private static DateTimeOffset? DateEndUtc(DateTime? value)
    {
        return value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc))
            : null;
    }
}
