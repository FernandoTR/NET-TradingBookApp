using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves the source reference and the verifiable catalog evidence used to identify a figure concept.</summary>
/// <param name="MappingVersion">The semantic mapping definition applied.</param>
/// <param name="SourceReference">The original observation, including its original ID, provenance and dates.</param>
/// <param name="ResolvedReference">The reference with the independently calculated catalog ID, when a unique consistent row is found.</param>
/// <param name="MatchedFigure">The matched catalog row, when uniquely identified.</param>
/// <param name="Concept">The resolved semantic concept, or explicit unavailable evidence with a reason.</param>
/// <param name="Candidates">The catalog rows considered, retaining ambiguity and conflicting arguments.</param>
/// <param name="ReasonCodes">The identifiable reasons for the resolution outcome.</param>
public sealed record FigureResolutionResultDto(
    string MappingVersion,
    CatalogReferenceDto SourceReference,
    CatalogReferenceDto? ResolvedReference,
    FigureCatalogMatchDto? MatchedFigure,
    EvidenceValueDto<FigureConceptKind> Concept,
    IReadOnlyList<FigureCatalogMatchDto> Candidates,
    IReadOnlyList<string> ReasonCodes
)
{
    /// <summary>Whether both a unique catalog reference and a known semantic concept are available.</summary>
    public bool IsResolved => ResolvedReference is not null && MatchedFigure is not null &&
        Concept.Availability == EvidenceAvailability.Present && Concept.Value.HasValue;

    /// <summary>Whether the concept is the pivot-line figure; null means it is not determinable.</summary>
    public bool? IsPivotLine => Concept.Availability == EvidenceAvailability.Present && Concept.Value.HasValue
        ? Concept.Value == FigureConceptKind.PivotLine
        : null;
}
