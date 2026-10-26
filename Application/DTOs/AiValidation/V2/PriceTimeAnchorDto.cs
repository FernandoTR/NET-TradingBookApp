namespace Application.DTOs.AiValidation.V2;

/// <summary>Represents a supplied price/time anchor without constructing map geometry.</summary>
/// <param name="ReferenceCode">A business reference identifying this anchor, not a file identifier.</param>
/// <param name="Price">The anchor price with its own availability and provenance.</param>
/// <param name="TimeUtc">The anchor's position in time; its confirmation can become available later.</param>
public sealed record PriceTimeAnchorDto(
    string ReferenceCode,
    EvidenceValueDto<decimal> Price,
    EvidenceValueDto<DateTimeOffset> TimeUtc
);
