namespace Application.DTOs.AiValidation.V2;

/// <summary>Associates one calculation's input, complete definition, internal classification and ordinal result.</summary>
/// <param name="Input">The observations supplied to this calculation, including original figure and manual-resolution evidence.</param>
/// <param name="Definition">The versioned rules and position table used by the same evaluation.</param>
/// <param name="Classification">The internal contextual classification, including known base context and missing-information reasons.</param>
/// <param name="Score">The position, decimal score and Grade produced from that classification.</param>
public sealed record ContextualScoreEvaluationDto(
    ContextualScoreInputDto Input,
    ContextualScoreDefinitionDto Definition,
    ScenarioGradeResultDto Classification,
    ContextualScoreResultDto Score
);
