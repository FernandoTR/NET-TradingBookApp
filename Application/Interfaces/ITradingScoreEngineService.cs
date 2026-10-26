using Application.DTOs.AiValidation.V2;
using Infrastructure;

namespace Application.Interfaces;

public interface ITradingScoreEngineService
{
    /// <summary>Evaluates contextual evidence without modifying legacy order score fields.</summary>
    /// <param name="input">The contextual observations, independently of manual Scenery.</param>
    /// <returns>The same evaluation's decimal score, classification and definition.</returns>
    ContextualScoreEvaluationDto Evaluate(ContextualScoreInputDto input);

    /// <summary>Initializes a legacy score for compatibility with existing creation consumers.</summary>
    /// <param name="order">The order whose legacy fields are explicitly being calculated.</param>
    void Evaluate(Order order);
}
