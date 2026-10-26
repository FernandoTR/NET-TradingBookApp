namespace Application.DTOs.AiValidation.V2;

/// <summary>Stores explicitly reviewed graphical state and the manual Scenery code, separately from calculated Grade.</summary>
/// <param name="Evaluation">The identified normative graphical evaluation, when available.</param>
/// <param name="ManualScenarioCode">The normalized/catalog scenario code selected manually, when recorded.</param>
public sealed record AiValidationMetricGraphicalSnapshotDto(
    SetupEvaluationResultDto? Evaluation,
    string? ManualScenarioCode
);
