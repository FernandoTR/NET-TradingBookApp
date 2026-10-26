using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Supplies contextual observations to the classifier without using the user's manual Scenery or Grade as input.</summary>
/// <param name="MainFigure">The versioned catalog resolution for the primary figure.</param>
/// <param name="Direction">The semantic trade direction to which base context and factor effects refer.</param>
/// <param name="BaseContext">The evidenced qualitative base assessment; unknown assessments are not inferred from stage or figure labels.</param>
/// <param name="Evidence">The structural arguments, convergences, factors and their review completeness.</param>
/// <param name="HasSufficientConvergence">An explicit assessment of sufficient directionally favorable convergence, not a count-based threshold.</param>
/// <param name="PredominanceResolution">A reasoned human resolution of opposing factors in a neutral context, when available.</param>
public sealed record ContextualScoreInputDto(
    FigureResolutionResultDto MainFigure,
    EvidenceValueDto<TradeDirection> Direction,
    EvidenceValueDto<ContextualBaseContext> BaseContext,
    ContextualEvidenceDto Evidence,
    EvidenceValueDto<bool> HasSufficientConvergence,
    ContextualPredominanceResolutionDto? PredominanceResolution = null
);
