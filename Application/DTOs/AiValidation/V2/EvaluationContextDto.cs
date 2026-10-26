using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Identifies the proposed setup without inferring its stage, frame or temporal modality.</summary>
/// <param name="InstrumentId">The instrument selected for evaluation.</param>
/// <param name="DirectionId">The selected trade direction from the direction catalog.</param>
/// <param name="StageId">The observed setup stage, independently of the zone's origin stage.</param>
/// <param name="TriggerFrameId">The observed trigger timeframe, independently of the zone's origin frame.</param>
/// <param name="SetupAtUtc">The business moment of the setup, not the time of a later evaluation or review.</param>
/// <param name="TimingMode">The explicitly supplied close, intrabar or anticipated-close modality.</param>
public sealed record EvaluationContextDto(
    int InstrumentId,
    int DirectionId,
    EvidenceValueDto<int> StageId,
    EvidenceValueDto<int> TriggerFrameId,
    EvidenceValueDto<DateTimeOffset> SetupAtUtc,
    EvidenceValueDto<EvaluationTimingMode> TimingMode
);
