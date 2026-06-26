using Domain.Enums;

namespace Application.DTOs.AiValidation;

public class AiValidationRuleResultDto
{
    public string RuleCode { get; set; } = null!;

    public string RuleName { get; set; } = null!;

    public ValidationRuleResult Result { get; set; }

    public decimal Weight { get; set; }

    public decimal ScoreObtained { get; set; }

    public string? Evidence { get; set; }

    public ValidationSource Source { get; set; }
}
