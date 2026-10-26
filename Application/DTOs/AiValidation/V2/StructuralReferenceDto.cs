using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Identifies one structure or additional map reference supporting a later contextual classification.
/// A reference alone does not establish convergence or determine whether it favors the trade direction.
/// </summary>
/// <param name="ReferenceCode">The structure's business reference, independently of any file or database ID.</param>
/// <param name="Kind">The observed price-map element.</param>
/// <param name="Figure">The figure concept and normalized catalog ID, if applicable.</param>
/// <param name="Direction">The observed structural direction, not the requested Long/Short direction.</param>
/// <param name="FrameId">The structure's timeframe, which may differ from the zone and trigger frames.</param>
/// <param name="StageId">The structure's stage, when determinable.</param>
/// <param name="Location">The supplied support, resistance or middle relationship in the reviewed context.</param>
/// <param name="InfluenceBounds">The supplied boundary prices and their reference moment.</param>
/// <param name="Anchors">Identified structural anchors; an empty list does not establish absence or completeness.</param>
/// <param name="IsActive">The observed activity of this reference at the setup moment.</param>
public sealed record StructuralReferenceDto(
    string ReferenceCode,
    EvidenceValueDto<MapReferenceKind> Kind,
    CatalogReferenceDto Figure,
    EvidenceValueDto<StructureDirection> Direction,
    EvidenceValueDto<int> FrameId,
    EvidenceValueDto<int> StageId,
    EvidenceValueDto<LocationType> Location,
    ZoneInfluenceBoundsDto InfluenceBounds,
    IReadOnlyList<PriceTimeAnchorDto> Anchors,
    EvidenceValueDto<bool> IsActive
);
