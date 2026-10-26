using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves an explicitly supplied contextual factor without turning it into a universal graphical requirement.</summary>
/// <param name="ReferenceCode">The business reference identifying this factor observation.</param>
/// <param name="FactorCode">The identified factor, such as volume, RSI or divergence.</param>
/// <param name="Effect">Its interpretation for the selected trade direction, with availability, provenance and dates.</param>
/// <param name="ObservedValue">A quantitative observation when applicable; qualitative factors can mark it not applicable.</param>
/// <param name="FrameId">The frame of the observation, when applicable and known.</param>
/// <param name="EvidenceReferences">The supplied observations supporting this interpretation.</param>
/// <param name="Rationale">The explanation of the factor's interpretation, when supplied.</param>
public sealed record ContextualFactorEvidenceDto(
    string ReferenceCode,
    string FactorCode,
    EvidenceValueDto<ContextualFactorEffect> Effect,
    EvidenceValueDto<decimal> ObservedValue,
    EvidenceValueDto<int> FrameId,
    IReadOnlyList<string> EvidenceReferences,
    string? Rationale
);
