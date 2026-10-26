using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Defines one contextual classification rule without numerical weights or grade bands.</summary>
/// <param name="Code">The stable rule identifier.</param>
/// <param name="BaseContext">The base context to which this rule applies.</param>
/// <param name="HasFavorableFactors">The required favorable-factor presence, or null when irrelevant to the rule.</param>
/// <param name="HasContraryFactors">The required contrary-factor presence, or null when irrelevant to the rule.</param>
/// <param name="HasSufficientConvergence">The required convergence assessment, or null when irrelevant to the rule.</param>
/// <param name="ResolvedPredominance">The manual predominance needed for a resolved neutral conflict.</param>
/// <param name="Grade">The resulting Grade, or null for a pending contextual conflict.</param>
/// <param name="Justification">The contextual meaning of this rule.</param>
/// <param name="NormativeReferences">The documentary rules and agreed application conventions supporting this definition.</param>
public sealed record ContextualClassificationRuleDto(
    string Code,
    ContextualBaseContext BaseContext,
    bool? HasFavorableFactors,
    bool? HasContraryFactors,
    bool? HasSufficientConvergence,
    ContextualFactorEffect? ResolvedPredominance,
    GradeType? Grade,
    string Justification,
    IReadOnlyList<string> NormativeReferences
);
