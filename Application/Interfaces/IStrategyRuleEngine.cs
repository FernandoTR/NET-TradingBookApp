using Application.DTOs.AiValidation;

namespace Application.Interfaces;

public interface IStrategyRuleEngine
{
    IReadOnlyList<AiValidationRuleResultDto> Evaluate(NormalizedTradeSetupDto setup);
}
