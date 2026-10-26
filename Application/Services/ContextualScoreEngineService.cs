using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

/// <summary>Normalizes a single contextual classification using the explicit, independently versioned ordinal table.</summary>
/// <param name="classifier">The classifier providing both the rule definition and the contextual assessment.</param>
public sealed class ContextualScoreEngineService(IContextualScenarioClassifier classifier) : IContextualScoreEngineService
{
    private readonly IContextualScenarioClassifier _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));

    /// <inheritdoc />
    public ContextualScoreEvaluationDto Evaluate(ContextualScoreInputDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var definition = _classifier.Definition;
        ValidateDefinition(definition);
        var classification = _classifier.Evaluate(input);
        if (!string.Equals(classification.ClassificationRulesVersion, definition.ClassificationRulesVersion, StringComparison.Ordinal))
            throw new InvalidOperationException("Classification and ordinal score must use the same rule definition.");
        if (!Enum.IsDefined(classification.State) ||
            (classification.State == ContextualScoreState.Complete) != classification.Grade.HasValue)
            throw new InvalidOperationException("Only a complete contextual classification can provide a Grade.");

        var components = classification.Components.ToArray();
        if (components.Any(component => !Enum.IsDefined(component.Availability)))
            throw new InvalidOperationException("Contextual components must use defined availability states.");

        var applicable = components.Where(component => component.Availability != EvidenceAvailability.NotApplicable ||
            string.IsNullOrWhiteSpace(component.ReasonCode)).ToArray();
        var expected = applicable.Length;
        var evaluated = applicable.Count(component => component.Availability is EvidenceAvailability.Present or EvidenceAvailability.Absent);
        int? rank = null;
        decimal? value = null;
        GradeType? grade = null;
        if (classification.State == ContextualScoreState.Complete)
        {
            if (expected == 0 || evaluated != expected)
                throw new InvalidOperationException("A complete contextual score requires all applicable components to be determined.");

            rank = definition.Positions.Single(position => position.Grade == classification.Grade).ContextRank;
            grade = definition.Positions.Single(position => position.ContextRank == rank).Grade;
            value = 100m * rank.Value / 6m;
        }

        var score = new ContextualScoreResultDto(definition.ScoreVersion, classification.State, rank, value, grade,
            expected, evaluated, Array.AsReadOnly(components));
        return new ContextualScoreEvaluationDto(input, definition, classification, score);
    }

    private static void ValidateDefinition(ContextualScoreDefinitionDto definition)
    {
        if (definition is null || string.IsNullOrWhiteSpace(definition.ScoreVersion) ||
            string.IsNullOrWhiteSpace(definition.ClassificationRulesVersion) || definition.Positions is null ||
            definition.Positions.Count != 7 || definition.Positions.Any(position => position.ContextRank is < 0 or > 6) ||
            definition.Positions.Select(position => position.ContextRank).Distinct().Count() != 7 ||
            !definition.Positions.Select(position => position.Grade).ToHashSet().SetEquals(Enum.GetValues<GradeType>()))
            throw new InvalidOperationException("The contextual definition requires seven unique Grade positions from 0 to 6 and explicit versions.");
    }
}
