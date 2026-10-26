using Application.DTOs.AiValidation.V2;

namespace Application.Interfaces;

/// <summary>Produces contextual position, decimal score and Grade from one typed evaluation.</summary>
public interface IContextualScoreEngineService
{
    /// <summary>Classifies the input once and uses the same definition for the ordinal score and its trace.</summary>
    /// <param name="input">The contextual evidence, independently of the user's manual Scenery.</param>
    /// <returns>The complete calculation, preserving unknown totals and Grade for incomplete evidence.</returns>
    ContextualScoreEvaluationDto Evaluate(ContextualScoreInputDto input);
}
