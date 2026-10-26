using Application.DTOs.AiValidation.V2;
using Domain.Enums;

namespace Application.Common;

/// <summary>Provides the agreed, versioned contextual rules and ordinal positions.</summary>
public static class ContextualScoreDefinitions
{
    /// <summary>The initial application convention; it expresses contextual order, not effectiveness or win probability.</summary>
    public static ContextualScoreDefinitionDto V1 { get; } = new(
        "contextual-rules-v1",
        "contextual-ordinal-v1",
        Array.AsReadOnly<ContextualClassificationRuleDto>(
        [
            Rule("FAVORABLE_CONVERGENCE", ContextualBaseContext.Favorable, null, false, true, null, GradeType.A,
                "Contexto favorable con convergencia suficiente y sin elementos contrarios."),
            Rule("FAVORABLE_CONTRARY_FACTOR", ContextualBaseContext.Favorable, null, true, null, null, GradeType.AB,
                "Contexto favorable con algún elemento contrario."),
            Rule("FAVORABLE_INSUFFICIENT_CONVERGENCE", ContextualBaseContext.Favorable, null, false, false, null, GradeType.AB,
                "Entrada favorable sin convergencia suficiente para A."),
            Rule("NEUTRAL_FAVORABLE_FACTOR", ContextualBaseContext.Neutral, true, false, null, null, GradeType.BA,
                "Contexto neutral con algún elemento favorable."),
            Rule("NEUTRAL_NO_MODIFIERS", ContextualBaseContext.Neutral, false, false, null, null, GradeType.B,
                "Contexto neutral sin modificadores favorables ni contrarios."),
            Rule("NEUTRAL_CONTRARY_FACTOR", ContextualBaseContext.Neutral, false, true, null, null, GradeType.BC,
                "Contexto neutral con algún elemento contrario."),
            Rule("CONTRARY_FAVORABLE_FACTOR", ContextualBaseContext.Contrary, true, null, null, null, GradeType.CB,
                "Contexto contrario a la estructura con algún elemento favorable."),
            Rule("CONTRARY_NO_FAVORABLE_FACTORS", ContextualBaseContext.Contrary, false, null, null, null, GradeType.C,
                "Operación contra la estructura sin elementos favorables."),
            Rule("NEUTRAL_OPPOSING_FACTORS_PENDING", ContextualBaseContext.Neutral, true, true, null, null, null,
                "Factores favorables y contrarios sin predominio resuelto manualmente."),
            Rule("NEUTRAL_RESOLVED_FAVORABLE", ContextualBaseContext.Neutral, true, true, null, ContextualFactorEffect.Favorable, GradeType.BA,
                "Se conservan ambos argumentos y la revisión fundamenta un predominio favorable."),
            Rule("NEUTRAL_RESOLVED_CONTRARY", ContextualBaseContext.Neutral, true, true, null, ContextualFactorEffect.Contrary, GradeType.BC,
                "Se conservan ambos argumentos y la revisión fundamenta un predominio contrario."),
            Rule("NEUTRAL_RESOLVED_NEUTRAL", ContextualBaseContext.Neutral, true, true, null, ContextualFactorEffect.Neutral, GradeType.B,
                "Se conservan ambos argumentos y la revisión fundamenta un predominio neutral.")
        ]),
        Array.AsReadOnly<ContextualGradePositionDto>(
        [
            new(GradeType.A, 6, "Favorable con convergencia suficiente."),
            new(GradeType.AB, 5, "Favorable con oposición o convergencia insuficiente."),
            new(GradeType.BA, 4, "Neutral con predominio favorable."),
            new(GradeType.B, 3, "Neutral sin predominio direccional."),
            new(GradeType.BC, 2, "Neutral con predominio contrario."),
            new(GradeType.CB, 1, "Contrario con algún elemento favorable."),
            new(GradeType.C, 0, "Contrario sin elementos favorables.")
        ]));

    private static ContextualClassificationRuleDto Rule(
        string code, ContextualBaseContext context, bool? favorable, bool? contrary, bool? convergence,
        ContextualFactorEffect? predominance, GradeType? grade, string justification) =>
        new(code, context, favorable, contrary, convergence, predominance, grade, justification,
            Array.AsReadOnly(new[] { "SPEC36:3.5", "CHECKLIST_TRADING:A", "CHECKLIST_TRADING:D:M1" }));
}
