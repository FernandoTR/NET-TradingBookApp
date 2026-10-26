namespace Application.DTOs.AiValidation.V2;

/// <summary>Stores applicable field observations for later denominator-aware aggregation.</summary>
/// <param name="Fields">Applicable field metrics; inapplicable fields are excluded from this collection.</param>
/// <param name="NotApplicableFieldCount">The count of fields excluded using a justified applicability decision.</param>
public sealed record AiValidationMetricFieldsSnapshotDto(
    IReadOnlyList<AiValidationFieldMetricDto> Fields,
    int NotApplicableFieldCount
);
