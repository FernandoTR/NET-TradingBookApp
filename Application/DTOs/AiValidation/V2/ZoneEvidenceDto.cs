using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>
/// Describes the selected working zone from supplied evidence, without evaluating validity or applying 1234.
/// Confirmation, activity and strike count remain independent observations.
/// </summary>
/// <param name="ReferenceCode">The zone's business reference, also usable in conflicts and result evidence paths.</param>
/// <param name="Kind">The kind of price-map zone, independently of its stage or catalog figure.</param>
/// <param name="Figure">The identified figure, if applicable; stage 1 does not require a stage-2 figure.</param>
/// <param name="InfluenceBounds">The supplied influence band and the moment to which it applies.</param>
/// <param name="Anchors">Identified anchors with individually available prices and times; an empty list does not prove absence.</param>
/// <param name="OriginFrameId">The frame in which the zone originates and stage-1 strikes are counted.</param>
/// <param name="OriginStageId">The zone's stage of origin, independently of the setup stage.</param>
/// <param name="CreatedAtUtc">The zone creation moment, with its original information availability.</param>
/// <param name="InvalidatedAtUtc">The invalidation moment, or explicit unknown/absence when appropriate.</param>
/// <param name="IsActive">Whether the zone remains active at the setup moment; not inferred from confirmation.</param>
/// <param name="IsConfirmed">The supplied confirmation state; false does not by itself invalidate a working line.</param>
/// <param name="StrikeCount">Strikes in the origin frame when applicable; zero is known and stage 2 may be not applicable.</param>
/// <param name="LastConfirmedPivotAtUtc">The most recent confirmed pivot relevant to a strike reset, when applicable.</param>
public sealed record ZoneEvidenceDto(
    string ReferenceCode,
    EvidenceValueDto<MapReferenceKind> Kind,
    CatalogReferenceDto Figure,
    ZoneInfluenceBoundsDto InfluenceBounds,
    IReadOnlyList<PriceTimeAnchorDto> Anchors,
    EvidenceValueDto<int> OriginFrameId,
    EvidenceValueDto<int> OriginStageId,
    EvidenceValueDto<DateTimeOffset> CreatedAtUtc,
    EvidenceValueDto<DateTimeOffset> InvalidatedAtUtc,
    EvidenceValueDto<bool> IsActive,
    EvidenceValueDto<bool> IsConfirmed,
    EvidenceValueDto<int> StrikeCount,
    EvidenceValueDto<DateTimeOffset> LastConfirmedPivotAtUtc
);
