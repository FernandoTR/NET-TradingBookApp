using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Explains one datum or check required by the applicable contextual rule; it is not a weighted score component.</summary>
/// <param name="Code">The requirement or factor-presence check.</param>
/// <param name="Availability">Availability of the checked information, with justified inapplicability when appropriate.</param>
/// <param name="Source">The origin of the observation or deterministic check.</param>
/// <param name="ReasonCode">The reason for the component outcome, especially missing or inapplicable information.</param>
/// <param name="EvidenceReferences">The business references or evidence paths actually considered.</param>
public sealed record ContextualScoreComponentDto(
    string Code,
    EvidenceAvailability Availability,
    ValidationSource? Source,
    string? ReasonCode,
    IReadOnlyList<string> EvidenceReferences
);
