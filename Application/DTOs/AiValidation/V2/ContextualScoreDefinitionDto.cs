using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Identifies the classification rules and explicit ordinal table used by one score definition.</summary>
/// <param name="ClassificationRulesVersion">The contextual-classification rule version.</param>
/// <param name="ScoreVersion">The ordinal-score convention version, independently of the classification version.</param>
/// <param name="Rules">The contextual rules, including pending and manually resolved neutral conflicts.</param>
/// <param name="Positions">The explicit Grade-to-position mapping and justifications.</param>
/// <param name="PivotLineBaseContext">The explicit base-context rule for a catalog-identified pivot-line figure.</param>
public sealed record ContextualScoreDefinitionDto(
    string ClassificationRulesVersion,
    string ScoreVersion,
    IReadOnlyList<ContextualClassificationRuleDto> Rules,
    IReadOnlyList<ContextualGradePositionDto> Positions,
    ContextualBaseContext PivotLineBaseContext = ContextualBaseContext.Neutral
);
