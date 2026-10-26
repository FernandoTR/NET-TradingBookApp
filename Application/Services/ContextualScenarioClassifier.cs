using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

/// <summary>Applies the agreed qualitative context rules, keeping unknown evidence distinct from absence.</summary>
public sealed class ContextualScenarioClassifier : IContextualScenarioClassifier
{
    /// <inheritdoc />
    public ContextualScoreDefinitionDto Definition => ContextualScoreDefinitions.V1;

    /// <inheritdoc />
    public ScenarioGradeResultDto Evaluate(ContextualScoreInputDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.MainFigure);
        ArgumentNullException.ThrowIfNull(input.Evidence);
        var trace = new EvaluationTrace();
        var context = ResolveBaseContext(input, trace);
        if (!Known(context))
            return Result(input, trace, context, null, ContextualScoreState.NotEvaluable);

        trace.Require("TRADE_DIRECTION", input.Direction, "TRADE_DIRECTION_REQUIRED", "direction");
        var primary = FindStructure(input.Evidence, input.Evidence.ClassificationStructureReferenceCode);
        if (primary is null)
            trace.Missing("CLASSIFICATION_STRUCTURE", "CLASSIFICATION_STRUCTURE_MISSING_OR_AMBIGUOUS", "evidence.structuralReferences");
        else
            CheckStructure(primary, trace, "PRIMARY_STRUCTURE");

        var factors = AnalyzeFactors(input.Evidence);
        bool? favorable = null;
        bool? contrary = null;
        bool? convergence = null;
        ContextualFactorEffect? predominance = null;

        if (context.Value is ContextualBaseContext.Neutral or ContextualBaseContext.Contrary)
            favorable = RequirePresence(trace, "HAS_FAVORABLE_FACTORS", factors.Favorable, factors, input.Evidence);
        else
            trace.Skip("HAS_FAVORABLE_FACTORS", "FAVORABLE_MODIFIERS_NOT_REQUIRED_FOR_FAVORABLE_BASE", "evidence.factors");

        if (context.Value is ContextualBaseContext.Neutral or ContextualBaseContext.Favorable)
            contrary = RequirePresence(trace, "HAS_CONTRARY_FACTORS", factors.Contrary, factors, input.Evidence);
        else
            trace.Skip("HAS_CONTRARY_FACTORS", "CONTRARY_MODIFIERS_NOT_REQUIRED_FOR_CONTRARY_BASE", "evidence.factors");

        if (context.Value == ContextualBaseContext.Favorable && contrary == false)
            convergence = CheckConvergence(input, trace);
        else
            trace.Skip("SUFFICIENT_CONVERGENCE", "CONVERGENCE_NOT_REQUIRED_FOR_APPLICABLE_RULE", "hasSufficientConvergence");

        if (context.Value == ContextualBaseContext.Neutral && favorable == true && contrary == true)
            predominance = CheckPredominance(input, factors, trace);
        else
            trace.Skip("MANUAL_PREDOMINANCE", "MANUAL_PREDOMINANCE_NOT_REQUIRED", "predominanceResolution");

        var rule = Definition.Rules.SingleOrDefault(rule => rule.BaseContext == context.Value &&
            Matches(rule.HasFavorableFactors, favorable) && Matches(rule.HasContraryFactors, contrary) &&
            Matches(rule.HasSufficientConvergence, convergence) && rule.ResolvedPredominance == predominance);
        if (rule is null)
            trace.Reasons.Add("CONTEXT_REQUIRED_EVIDENCE_MISSING");

        return Result(input, trace, context, rule,
            !trace.HasMissing && rule?.Grade is not null ? ContextualScoreState.Complete : ContextualScoreState.Partial);
    }

    private EvidenceValueDto<ContextualBaseContext> ResolveBaseContext(ContextualScoreInputDto input, EvaluationTrace trace)
    {
        var figure = input.MainFigure;
        if (input.Evidence.MainFigure != figure.SourceReference && input.Evidence.MainFigure != figure.ResolvedReference)
        {
            trace.Missing("MAIN_FIGURE", "FIGURE_EVIDENCE_MISMATCH", "mainFigure", "evidence.mainFigure");
            return UnknownContext("FIGURE_EVIDENCE_MISMATCH");
        }

        var source = figure.SourceReference;
        if (source.Availability is EvidenceAvailability.NotApplicable or EvidenceAvailability.Absent &&
            Validator.TryValidateObject(source, new ValidationContext(source), [], validateAllProperties: true))
        {
            trace.Components.Add(new("MAIN_FIGURE", source.Availability, source.Source, source.ReasonCode, ["mainFigure.sourceReference"]));
        }
        else if (!figure.IsResolved || !Known(figure.Concept) ||
            figure.ResolvedReference!.NormalizedId.Value != figure.MatchedFigure!.Id || figure.MatchedFigure.Id <= 0)
        {
            trace.Missing("MAIN_FIGURE", "FIGURE_NOT_RESOLVED", "mainFigure");
            trace.Reasons.AddRange(figure.ReasonCodes);
            return UnknownContext("FIGURE_NOT_RESOLVED");
        }
        else
        {
            trace.Components.Add(new("MAIN_FIGURE", EvidenceAvailability.Present, ValidationSource.DeterministicRule,
                "FIGURE_CATALOG_RESOLVED", ["mainFigure.matchedFigure", "mainFigure.mappingVersion"]));
        }

        if (figure.IsResolved && figure.IsPivotLine == true)
        {
            var context = new EvidenceValueDto<ContextualBaseContext>(EvidenceAvailability.Present, Definition.PivotLineBaseContext,
                ValidationSource.DeterministicRule, null, null, null, "PIVOT_LINE_BASE_NEUTRAL");
            trace.Require("BASE_CONTEXT", context, "BASE_CONTEXT_REQUIRED", "mainFigure.concept");
            if (Known(input.BaseContext) && input.BaseContext.Value != Definition.PivotLineBaseContext)
                trace.Missing("BASE_CONTEXT_CONSISTENCY", "PIVOT_BASE_CONTEXT_CONFLICT", "baseContext", "mainFigure.concept");
            return context;
        }

        trace.Require("BASE_CONTEXT", input.BaseContext, "BASE_CONTEXT_REQUIRED", "baseContext");
        return Known(input.BaseContext) ? input.BaseContext : UnknownContext("BASE_CONTEXT_REQUIRED");
    }

    private static bool? CheckConvergence(ContextualScoreInputDto input, EvaluationTrace trace)
    {
        var sufficient = trace.Require("SUFFICIENT_CONVERGENCE", input.HasSufficientConvergence,
            "CONVERGENCE_ASSESSMENT_REQUIRED", "hasSufficientConvergence");
        if (sufficient == false)
        {
            trace.Require("CONVERGENCE_REVIEW", input.Evidence.ConvergencesComplete, "CONVERGENCE_REVIEW_INCOMPLETE",
                "evidence.convergencesComplete", predicate: reviewed => reviewed);
        }
        else if (sufficient == true)
        {
            var witnesses = input.Evidence.Convergences.Where(convergence =>
                Known(convergence.IsPresent) && convergence.IsPresent.Value == true &&
                convergence.StructuralReferenceCodes.Count >= 2 &&
                convergence.StructuralReferenceCodes.Distinct(StringComparer.Ordinal).Count() == convergence.StructuralReferenceCodes.Count &&
                convergence.StructuralReferenceCodes.Contains(input.Evidence.ClassificationStructureReferenceCode, StringComparer.Ordinal) &&
                convergence.StructuralReferenceCodes.All(code => FindStructure(input.Evidence, code) is not null) &&
                HasReferences(convergence.EvidenceReferences)).ToArray();
            var witness = witnesses.FirstOrDefault(convergence => convergence.StructuralReferenceCodes.All(code =>
            {
                var structure = FindStructure(input.Evidence, code)!;
                return Known(structure.Direction) && Known(structure.FrameId) && structure.FrameId.Value > 0 &&
                    Known(structure.IsActive) && structure.IsActive.Value == true;
            })) ?? witnesses.FirstOrDefault();
            if (witness is null)
                trace.Missing("CONVERGENCE_ARGUMENTS", "CONVERGENCE_EVIDENCE_MISSING", "evidence.convergences");
            else
            {
                trace.Components.Add(new("CONVERGENCE_ARGUMENTS", EvidenceAvailability.Present, witness.IsPresent.Source,
                    "CONVERGENCE_ARGUMENTS_IDENTIFIED", [witness.ReferenceCode, .. witness.StructuralReferenceCodes]));
                foreach (var reference in witness.StructuralReferenceCodes)
                    CheckStructure(FindStructure(input.Evidence, reference)!, trace, $"CONVERGENCE_STRUCTURE:{reference}");
            }
        }

        return sufficient;
    }

    private static ContextualFactorEffect? CheckPredominance(ContextualScoreInputDto input, FactorAssessment factors, EvaluationTrace trace)
    {
        var resolution = input.PredominanceResolution;
        if (resolution is null)
        {
            trace.Missing("MANUAL_PREDOMINANCE", "CONTEXT_PREDOMINANCE_PENDING", "evidence.factors");
            return null;
        }

        if (!Validator.TryValidateObject(resolution, new ValidationContext(resolution), [], validateAllProperties: true))
        {
            trace.Missing("MANUAL_PREDOMINANCE", "CONTEXT_RESOLUTION_INVALID", "predominanceResolution");
            return null;
        }

        if (!factors.IsComplete)
        {
            trace.Missing("MANUAL_PREDOMINANCE", "CONTEXT_RESOLUTION_REQUIRES_COMPLETE_FACTOR_REVIEW", "evidence.factorsComplete", "evidence.factors");
            return null;
        }

        if (!SameArguments(resolution.FavorableArguments, factors.FavorableArguments) ||
            !SameArguments(resolution.ContraryArguments, factors.ContraryArguments))
        {
            trace.Missing("MANUAL_PREDOMINANCE", "CONTEXT_RESOLUTION_STALE", "predominanceResolution", "evidence.factors");
            return null;
        }

        trace.Components.Add(new("MANUAL_PREDOMINANCE", EvidenceAvailability.Present, ValidationSource.UserConfirmation,
            "CONTEXT_PREDOMINANCE_RESOLVED", ["predominanceResolution", .. factors.FavorableArguments.Select(factor => factor.ReferenceCode),
                .. factors.ContraryArguments.Select(factor => factor.ReferenceCode)]));
        trace.Reasons.Add("CONTEXT_PREDOMINANCE_RESOLVED");
        return resolution.Predominance;
    }

    private static FactorAssessment AnalyzeFactors(ContextualEvidenceDto evidence)
    {
        var favorable = new List<ContextualFactorEvidenceDto>();
        var contrary = new List<ContextualFactorEvidenceDto>();
        var identifiers = new HashSet<string>(StringComparer.Ordinal);
        var unknown = false;
        var invalidIdentifiers = false;
        foreach (var factor in evidence.Factors)
        {
            if (factor is null || string.IsNullOrWhiteSpace(factor.ReferenceCode) || !identifiers.Add(factor.ReferenceCode))
            {
                invalidIdentifiers = true;
                continue;
            }

            if (Valid(factor.Effect) && factor.Effect.Availability is EvidenceAvailability.Absent or EvidenceAvailability.NotApplicable)
                continue;

            if (string.IsNullOrWhiteSpace(factor.FactorCode) || !Known(factor.Effect) || !HasReferences(factor.EvidenceReferences) ||
                !(Known(factor.FrameId) && factor.FrameId.Value > 0 || Valid(factor.FrameId) && factor.FrameId.Availability == EvidenceAvailability.NotApplicable))
            {
                unknown = true;
                continue;
            }

            if (factor.Effect.Value == ContextualFactorEffect.Favorable) favorable.Add(factor);
            if (factor.Effect.Value == ContextualFactorEffect.Contrary) contrary.Add(factor);
        }

        var complete = !unknown && !invalidIdentifiers && Known(evidence.FactorsComplete) && evidence.FactorsComplete.Value == true;
        return new FactorAssessment(
            invalidIdentifiers ? null : favorable.Count > 0 ? true : complete ? false : null,
            invalidIdentifiers ? null : contrary.Count > 0 ? true : complete ? false : null,
            complete, invalidIdentifiers ? "FACTOR_REFERENCES_INVALID" : unknown ? "FACTOR_EVIDENCE_UNKNOWN" : "FACTOR_REVIEW_INCOMPLETE",
            favorable, contrary);
    }

    private static bool? RequirePresence(EvaluationTrace trace, string code, bool? value, FactorAssessment assessment, ContextualEvidenceDto evidence)
    {
        if (!value.HasValue)
        {
            trace.Missing(code, assessment.MissingReason, "evidence.factorsComplete", "evidence.factors");
            return null;
        }

        trace.Components.Add(new(code, EvidenceAvailability.Present, ValidationSource.DeterministicRule,
            value.Value ? "CONTEXTUAL_FACTOR_PRESENT" : "CONTEXTUAL_FACTOR_ABSENCE_CONFIRMED",
            ["evidence.factorsComplete", .. evidence.Factors.Select(factor => factor.ReferenceCode)]));
        return value;
    }

    private static void CheckStructure(StructuralReferenceDto structure, EvaluationTrace trace, string prefix)
    {
        trace.Require($"{prefix}_DIRECTION", structure.Direction, "STRUCTURE_DIRECTION_REQUIRED", structure.ReferenceCode);
        trace.Require($"{prefix}_FRAME", structure.FrameId, "STRUCTURE_FRAME_REQUIRED", structure.ReferenceCode, predicate: frame => frame > 0);
        trace.Require($"{prefix}_ACTIVE", structure.IsActive, "STRUCTURE_ACTIVITY_REQUIRED", structure.ReferenceCode, predicate: active => active);
    }

    private static StructuralReferenceDto? FindStructure(ContextualEvidenceDto evidence, string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return null;
        var matches = evidence.StructuralReferences.Where(structure => structure.ReferenceCode == reference).Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static bool SameArguments(IReadOnlyList<ContextualFactorEvidenceDto> reviewed, IReadOnlyList<ContextualFactorEvidenceDto> current) =>
        reviewed.Count == current.Count && reviewed.All(argument => current.Any(factor =>
            argument.ReferenceCode == factor.ReferenceCode && argument.FactorCode == factor.FactorCode &&
            argument.Effect == factor.Effect && argument.ObservedValue == factor.ObservedValue && argument.FrameId == factor.FrameId &&
            argument.Rationale == factor.Rationale && argument.EvidenceReferences.SequenceEqual(factor.EvidenceReferences, StringComparer.Ordinal)));

    private ScenarioGradeResultDto Result(ContextualScoreInputDto input, EvaluationTrace trace,
        EvidenceValueDto<ContextualBaseContext> context, ContextualClassificationRuleDto? rule, ContextualScoreState state) => new(
            Definition.ClassificationRulesVersion, state, context, state == ContextualScoreState.Complete ? rule!.Grade : null,
            rule?.Code, rule?.Justification, trace.Components.ToArray(), input.Evidence.StructuralReferences,
            input.Evidence.Convergences, input.Evidence.Factors, input.PredominanceResolution,
            trace.Reasons.Concat(rule is null ? [] : new[] { rule.Code }).Distinct(StringComparer.Ordinal).ToArray(),
            trace.Components.SelectMany(component => component.EvidenceReferences).Distinct(StringComparer.Ordinal).ToArray());

    private static bool Matches(bool? expected, bool? actual) => !expected.HasValue || expected == actual;
    private static bool HasReferences(IReadOnlyList<string>? references) => references is { Count: > 0 } && references.All(reference => !string.IsNullOrWhiteSpace(reference));
    private static bool Valid<T>(EvidenceValueDto<T>? evidence) where T : struct => evidence is not null &&
        !evidence.Validate(new ValidationContext(evidence)).Any() &&
        (!evidence.Value.HasValue || !typeof(T).IsEnum || Enum.IsDefined(typeof(T), evidence.Value.Value));
    private static bool Known<T>(EvidenceValueDto<T>? evidence) where T : struct =>
        Valid(evidence) && evidence!.Availability == EvidenceAvailability.Present && evidence.Value.HasValue;
    private static EvidenceValueDto<ContextualBaseContext> UnknownContext(string reason) =>
        new(EvidenceAvailability.Unknown, null, ValidationSource.DeterministicRule, null, null, null, reason);

    private sealed record FactorAssessment(bool? Favorable, bool? Contrary, bool IsComplete, string MissingReason,
        IReadOnlyList<ContextualFactorEvidenceDto> FavorableArguments, IReadOnlyList<ContextualFactorEvidenceDto> ContraryArguments);

    private sealed class EvaluationTrace
    {
        public List<ContextualScoreComponentDto> Components { get; } = [];
        public List<string> Reasons { get; } = [];
        public bool HasMissing { get; private set; }

        public T? Require<T>(string code, EvidenceValueDto<T>? evidence, string reason, string reference, Func<T, bool>? predicate = null) where T : struct
        {
            if (!Known(evidence) || predicate is not null && !predicate(evidence!.Value!.Value))
            {
                HasMissing = true;
                Reasons.Add(reason);
                Components.Add(new(code, Valid(evidence) && evidence!.Availability == EvidenceAvailability.Absent
                    ? EvidenceAvailability.Absent : EvidenceAvailability.Unknown, evidence?.Source, reason, [reference]));
                if (!string.IsNullOrWhiteSpace(evidence?.ReasonCode)) Reasons.Add(evidence.ReasonCode);
                return null;
            }

            Components.Add(new(code, EvidenceAvailability.Present, evidence!.Source, evidence.ReasonCode, [reference]));
            return evidence.Value;
        }

        public void Missing(string code, string reason, params string[] references)
        {
            HasMissing = true;
            Reasons.Add(reason);
            Components.Add(new(code, EvidenceAvailability.Unknown, ValidationSource.DeterministicRule, reason, references));
        }

        public void Skip(string code, string reason, string reference) =>
            Components.Add(new(code, EvidenceAvailability.NotApplicable, ValidationSource.DeterministicRule, reason, [reference]));
    }
}
