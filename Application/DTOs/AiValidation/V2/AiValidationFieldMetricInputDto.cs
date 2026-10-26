namespace Application.DTOs.AiValidation.V2;

/// <summary>Supplies the AI and human observations for one expected field and explicitly records review/applicability.</summary>
/// <param name="FieldCode">A stable business field code, independent of localized labels.</param>
/// <param name="IsApplicable">Whether this field belongs in this evaluation's extraction denominator.</param>
/// <param name="AiObservation">The original AI observation, unchanged by a later human correction.</param>
/// <param name="HumanObservation">The separate human observation or review disposition.</param>
/// <param name="WasReviewed">Whether a human actually reviewed this field; this is distinct from the value being unknown.</param>
/// <param name="ApplicabilityReasonCode">The reason for excluding a non-applicable field.</param>
/// <param name="ReviewReasonCode">The reason for a correction, completion, discard or review.</param>
/// <param name="AiEvidenceReference">A business reference to the AI observation when recorded.</param>
/// <param name="HumanEvidenceReference">A separate business reference to the human observation when recorded.</param>
public sealed record AiValidationFieldMetricInputDto(
    string FieldCode,
    bool IsApplicable,
    AiValidationFieldObservationDto AiObservation,
    AiValidationFieldObservationDto HumanObservation,
    bool WasReviewed,
    string? ApplicabilityReasonCode = null,
    string? ReviewReasonCode = null,
    string? AiEvidenceReference = null,
    string? HumanEvidenceReference = null
);
