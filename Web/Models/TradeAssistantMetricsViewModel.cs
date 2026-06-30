using Application.DTOs.AiValidation;

namespace Web.Models;

public sealed class TradeAssistantMetricsViewModel
{
    public string? ProviderName { get; set; }

    public string? ModelName { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public AiValidationMetricSummaryDto Summary { get; set; } = new();
}
