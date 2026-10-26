using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Defines an ordinal position independently of the numeric values of GradeType.</summary>
/// <param name="Grade">The existing application Grade value.</param>
/// <param name="ContextRank">The explicitly agreed internal position from 0 to 6.</param>
/// <param name="Justification">The contextual meaning of this position.</param>
public sealed record ContextualGradePositionDto(GradeType Grade, int ContextRank, string Justification);
