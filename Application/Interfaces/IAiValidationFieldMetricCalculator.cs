using Application.DTOs.AiValidation.V2;

namespace Application.Interfaces;

/// <summary>Calculates per-field extraction and review measures with explicit comparison denominators.</summary>
public interface IAiValidationFieldMetricCalculator
{
    /// <summary>Applies the versioned availability, comparison, correction and completion rules to applicable fields.</summary>
    /// <param name="fields">Field observations and review state for one validation population.</param>
    /// <returns>Field results and rates; every zero denominator has a null percentage.</returns>
    AiValidationFieldMetricMeasurementV2Dto Calculate(IReadOnlyList<AiValidationFieldMetricInputDto> fields);
}
