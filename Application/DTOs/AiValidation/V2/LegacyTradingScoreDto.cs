namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves legacy score values in their original types, independently of the new decimal ordinal score.</summary>
/// <param name="StructuralScore">The original legacy structural score, when recorded.</param>
/// <param name="TotalScore">The original legacy double value, without ordinal normalization or integer conversion.</param>
/// <param name="Grade">The original grade text, without replacing it with a newly calculated classification.</param>
/// <param name="ScoreVersion">The original version only when recorded; unknown historical versions remain null.</param>
public sealed record LegacyTradingScoreDto(short? StructuralScore, double? TotalScore, string? Grade, string? ScoreVersion = null);
