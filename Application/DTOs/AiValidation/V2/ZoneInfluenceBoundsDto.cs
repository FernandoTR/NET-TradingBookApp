namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Holds supplied influence boundaries at a particular moment, including for inclined lines.
/// It does not interpolate anchors, choose a price scale or introduce a width tolerance.
/// </summary>
/// <param name="LowerPrice">The observed lower boundary at the referenced moment.</param>
/// <param name="UpperPrice">The observed upper boundary at the referenced moment.</param>
/// <param name="AtUtc">The moment to which these prices apply, independently of when they became known.</param>
public sealed record ZoneInfluenceBoundsDto(
    EvidenceValueDto<decimal> LowerPrice,
    EvidenceValueDto<decimal> UpperPrice,
    EvidenceValueDto<DateTimeOffset> AtUtc
);
