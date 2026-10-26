using Application.DTOs.AiValidation.V2;

namespace Application.Interfaces;

/// <summary>Classifies a contextual observation set using versioned qualitative rules.</summary>
public interface IContextualScenarioClassifier
{
    /// <summary>The rules and explicit ordinal positions used by this classifier.</summary>
    ContextualScoreDefinitionDto Definition { get; }

    /// <summary>Evaluates base context and relevant modifiers, preserving uncertainty and manual-resolution arguments.</summary>
    /// <param name="input">The normalized figure and contextual evidence, excluding the manual Scenery.</param>
    /// <returns>The contextual Grade when determinable, together with versions, reasons and evidence references.</returns>
    ScenarioGradeResultDto Evaluate(ContextualScoreInputDto input);
}
