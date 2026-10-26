using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Describes one proposed trigger from supplied observations, independently of its catalog ID or graphical validity.
/// Candle closure, phase completion and ordered intrabar evidence are separate facts.
/// </summary>
/// <param name="ReferenceCode">The trigger's business reference.</param>
/// <param name="ZoneReferenceCode">The chosen map zone against which the trigger is interpreted.</param>
/// <param name="Concept">The identified trigger concept and independently normalized catalog ID.</param>
/// <param name="Family">The observed reversal or continuation family.</param>
/// <param name="Candles">The relevant candles; list order does not prove an intrabar sequence.</param>
/// <param name="IsClosed">Explicit evidence of pattern closure, without imposing closure on continuation triggers.</param>
/// <param name="MovementExtreme">The relevant preceding movement extreme, independently of pattern identification.</param>
/// <param name="Phases">Observed phases, retaining missing information and restarted sequences.</param>
/// <param name="VariantEvidence">One alternative's specific observations, or null if the variant cannot yet be identified.</param>
public sealed record TriggerEvidenceDto(
    string ReferenceCode,
    string ZoneReferenceCode,
    CatalogReferenceDto Concept,
    EvidenceValueDto<TriggerFamily> Family,
    IReadOnlyList<TriggerCandleEvidenceDto> Candles,
    EvidenceValueDto<bool> IsClosed,
    PriceTimeAnchorDto MovementExtreme,
    IReadOnlyList<TriggerPhaseEvidenceDto> Phases,
    TriggerVariantEvidenceDto? VariantEvidence
);
