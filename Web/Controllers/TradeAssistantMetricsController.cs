using Application.DTOs.AiValidation;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[Authorize]
public class TradeAssistantMetricsController : Controller
{
    private readonly IAiTradeValidationMetricService _metricService;
    private readonly ILogService _logService;

    public TradeAssistantMetricsController(
        IAiTradeValidationMetricService metricService,
        ILogService logService)
    {
        _metricService = metricService;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? providerName,
        string? modelName,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        try
        {
            var filter = new AiValidationMetricFilterDto
            {
                ProviderName = providerName,
                ModelName = modelName,
                From = from?.Date,
                To = to?.Date.AddDays(1).AddTicks(-1)
            };

            var summary = await _metricService.GetSummaryAsync(filter, cancellationToken);

            return View(new TradeAssistantMetricsViewModel
            {
                ProviderName = providerName,
                ModelName = modelName,
                From = from,
                To = to,
                Summary = summary
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: TradeAssistantMetrics, Action: {nameof(Index)}", ex);
            TempData["TradeAssistantMetricsMessage"] = "No fue posible cargar las metricas.";

            return View(new TradeAssistantMetricsViewModel());
        }
    }
}
