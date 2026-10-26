using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

/// <summary>Resolves figures using exact normalized business aliases verified against Cat_Figure.</summary>
/// <param name="catFigureService">The current catalog, including references needed for historical observations.</param>
public sealed class ContextualFigureResolver(ICatFigureService catFigureService) : IContextualFigureResolver
{
    /// <summary>The version identifying the verified code/description aliases and matching rules.</summary>
    public const string MappingVersion = "figure-semantics-v1";

    private readonly ICatFigureService _catFigureService = catFigureService ?? throw new ArgumentNullException(nameof(catFigureService));

    // Codes and descriptions verified by a read-only development-catalog query on 2026-10-23.
    // IDs deliberately do not participate in this definition. The SQL verification script is delivered with SPEC 36.
    private static readonly IReadOnlyDictionary<string, FigureConceptKind> Aliases = CreateAliases();

    /// <inheritdoc />
    public async Task<FigureResolutionResultDto> ResolveAsync(CatalogReferenceDto reference, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reference);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Validator.TryValidateObject(reference, new ValidationContext(reference), [], validateAllProperties: true))
        {
            return Unresolved(reference, "FIGURE_REFERENCE_INVALID");
        }

        if (reference.Availability != EvidenceAvailability.Present)
        {
            return Unresolved(reference, reference.ReasonCode!, availability: reference.Availability);
        }

        var catalog = (await _catFigureService.GetAllAsync())
            .Select(figure => new FigureCatalogMatchDto(figure.Id, figure.Code, figure.Description, figure.IsActived))
            .ToArray();
        cancellationToken.ThrowIfCancellationRequested();

        var constraints = new List<FigureCatalogMatchDto[]>();
        if (reference.NormalizedId.Availability == EvidenceAvailability.Present)
        {
            var idMatches = catalog.Where(figure => figure.Id == reference.NormalizedId.Value).ToArray();
            if (idMatches.Length == 0)
            {
                return Unresolved(reference, "FIGURE_CATALOG_ID_NOT_FOUND");
            }

            constraints.Add(idMatches);
        }

        foreach (var text in new[] { reference.ConceptCode, reference.ObservedLabel }
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => Normalize(text!))
            .Distinct(StringComparer.Ordinal))
        {
            constraints.Add(catalog.Where(figure => Matches(figure, text)).ToArray());
        }

        var candidates = constraints.SelectMany(matches => matches).Distinct().OrderBy(figure => figure.Id).ToArray();
        var consistent = constraints.Count == 0
            ? []
            : constraints.Skip(1).Aggregate(constraints[0].AsEnumerable(), (matches, next) => matches.Intersect(next)).ToArray();

        if (consistent.Length == 0)
        {
            return Unresolved(reference, candidates.Length == 0 ? "FIGURE_CATALOG_NOT_FOUND" : "FIGURE_REFERENCE_CONFLICT", candidates);
        }

        if (consistent.Length > 1)
        {
            return Unresolved(reference, "FIGURE_CATALOG_AMBIGUOUS", candidates);
        }

        var matched = consistent[0];
        var codeConcept = FindConcept(matched.Code);
        var descriptionConcept = FindConcept(matched.Description);
        if (codeConcept.HasValue && descriptionConcept.HasValue && codeConcept != descriptionConcept)
        {
            return Unresolved(reference, "FIGURE_CATALOG_SEMANTIC_CONFLICT", candidates);
        }

        var calculatedAt = DateTimeOffset.UtcNow;
        var resolved = reference with
        {
            NormalizedId = new(EvidenceAvailability.Present, matched.Id, ValidationSource.DeterministicRule,
                null, calculatedAt, null, null)
        };
        var concept = codeConcept ?? descriptionConcept;
        var conceptEvidence = new EvidenceValueDto<FigureConceptKind>(
            concept.HasValue ? EvidenceAvailability.Present : EvidenceAvailability.Unknown,
            concept, ValidationSource.DeterministicRule, null, calculatedAt, null,
            concept.HasValue ? null : "FIGURE_CONCEPT_NOT_MAPPED");

        return new FigureResolutionResultDto(MappingVersion, reference, resolved, matched, conceptEvidence, candidates,
            [concept.HasValue ? "FIGURE_CATALOG_RESOLVED" : "FIGURE_CONCEPT_NOT_MAPPED"]);
    }

    private static bool Matches(FigureCatalogMatchDto figure, string text)
    {
        if (Normalize(figure.Code ?? string.Empty) == text || Normalize(figure.Description ?? string.Empty) == text)
        {
            return true;
        }

        return Aliases.TryGetValue(text, out var concept) &&
            (FindConcept(figure.Code) == concept || FindConcept(figure.Description) == concept);
    }

    private static FigureConceptKind? FindConcept(string? text) =>
        !string.IsNullOrWhiteSpace(text) && Aliases.TryGetValue(Normalize(text), out var concept) ? concept : null;

    private static FigureResolutionResultDto Unresolved(
        CatalogReferenceDto reference,
        string reason,
        IReadOnlyList<FigureCatalogMatchDto>? candidates = null,
        EvidenceAvailability availability = EvidenceAvailability.Unknown) => new(
            MappingVersion, reference, null, null,
            new(availability, null, ValidationSource.DeterministicRule, null, DateTimeOffset.UtcNow, null, reason),
            candidates ?? [], [reason]);

    private static IReadOnlyDictionary<string, FigureConceptKind> CreateAliases()
    {
        var definitions = new[]
        {
            (FigureConceptKind.PivotLine, "LP", "LINEA PIVOTE"),
            (FigureConceptKind.AscendingChannel, "CA", "CANAL ASCENDENTE"),
            (FigureConceptKind.DescendingChannel, "CD", "CANAL DESCENDENTE"),
            (FigureConceptKind.NeutralChannel, "CN", "CANAL NEUTRO"),
            (FigureConceptKind.AscendingTriangle, "TA", "TRIANGULO ASCENDENTE"),
            (FigureConceptKind.DescendingTriangle, "TD", "TRIANGULO DESCENDENTE"),
            (FigureConceptKind.SymmetricTriangle, "TS", "TRIANGULO SIMETRICO"),
            (FigureConceptKind.CupAndHandle, "TM", "TAZA Y MANGO"),
            (FigureConceptKind.TrendLine, "LT", "LINEA TENDENCIA")
        };
        var aliases = new Dictionary<string, FigureConceptKind>(StringComparer.Ordinal);
        foreach (var (concept, code, description) in definitions)
        {
            aliases.Add(Normalize(concept.ToString()), concept);
            aliases.Add(Normalize(code), concept);
            aliases.Add(Normalize(description), concept);
        }

        return aliases;
    }

    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
