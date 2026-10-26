namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves a relevant candle without inferring its internal price sequence from OHLC.</summary>
/// <param name="ReferenceCode">A business reference identifying the candle.</param>
/// <param name="RoleCode">Its supplied pattern role, such as left range, head or right shoulder.</param>
/// <param name="Open">The observed opening price.</param>
/// <param name="High">The observed maximum price.</param>
/// <param name="Low">The observed minimum price.</param>
/// <param name="Close">The observed closing or developing price; it does not establish that the candle closed.</param>
/// <param name="OpenedAtUtc">The candle opening moment, when known.</param>
/// <param name="ClosedAtUtc">The actual candle closing moment, when known; not an assumed scheduled close.</param>
/// <param name="IsClosed">Explicit evidence of closure, independently of the OHLC values.</param>
public sealed record TriggerCandleEvidenceDto(
    string ReferenceCode,
    string RoleCode,
    EvidenceValueDto<decimal> Open,
    EvidenceValueDto<decimal> High,
    EvidenceValueDto<decimal> Low,
    EvidenceValueDto<decimal> Close,
    EvidenceValueDto<DateTimeOffset> OpenedAtUtc,
    EvidenceValueDto<DateTimeOffset> ClosedAtUtc,
    EvidenceValueDto<bool> IsClosed
);
