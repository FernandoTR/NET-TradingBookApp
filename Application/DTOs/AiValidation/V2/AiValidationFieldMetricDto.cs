using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Describes one applicable field's AI/user agreement without assigning a result to unknown values.</summary>
/// <param name="FieldCode">The stable business field identifier.</param>
/// <param name="AiAvailability">The original extraction availability.</param>
/// <param name="HumanAvailability">The separate user-review availability.</param>
/// <param name="AiSource">The AI observation source when recorded.</param>
/// <param name="HumanSource">The human observation source when recorded.</param>
/// <param name="Matches">True/false only for comparable, reviewed observations; otherwise null.</param>
/// <param name="ChangeKind">The explicit no-change, correction, completion, discard or no-review classification.</param>
/// <param name="ReasonCode">The reason for the change or non-comparable state.</param>
/// <param name="AiEvidenceReference">An optional business evidence reference for the AI value.</param>
/// <param name="HumanEvidenceReference">An optional business evidence reference for the human value.</param>
public sealed record AiValidationFieldMetricDto(
    string FieldCode,
    EvidenceAvailability AiAvailability,
    EvidenceAvailability HumanAvailability,
    ValidationSource? AiSource,
    ValidationSource? HumanSource,
    bool? Matches,
    AiValidationFieldChangeKind ChangeKind,
    string? ReasonCode,
    string? AiEvidenceReference = null,
    string? HumanEvidenceReference = null
);
