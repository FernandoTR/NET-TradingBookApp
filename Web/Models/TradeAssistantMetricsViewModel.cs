using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;

namespace Web.Models;

public sealed class TradeAssistantMetricsViewModel
{
    public AiValidationMetricSummaryDto LegacySummary { get; set; } = new();

    public AiValidationMetricSummaryV2Dto SummaryV2 { get; set; } = new(0, 0, 0, [], [], []);

    public AiValidationMetricFilterV2Dto Filter { get; set; } = new();

    public bool IsV2Available { get; set; } = true;

    public string? V2UnavailableMessage { get; set; }
}
