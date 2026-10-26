using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.AiValidation.V2;

/// <summary>Records a manual real-trading decision for a specific evaluation revision.</summary>
/// <param name="EvaluationReferenceCode">The business identity of the reviewed evaluation.</param>
/// <param name="EvaluationRevision">The reviewed revision; it may be unknown only while the review is pending.</param>
/// <param name="State">The manually recorded review state, pending by default.</param>
/// <param name="ReviewerUserId">The responsible reviewer, required for a recorded decision.</param>
/// <param name="ReviewedAtUtc">When the manual decision was made, required for a recorded decision.</param>
/// <param name="Rationale">The decision's foundation, required for either enabling or not enabling real trading.</param>
public sealed record RealTradingReviewDto(
    string EvaluationReferenceCode,
    int? EvaluationRevision = null,
    RealTradingReviewState State = RealTradingReviewState.Pending,
    string? ReviewerUserId = null,
    DateTimeOffset? ReviewedAtUtc = null,
    string? Rationale = null
) : IValidatableObject
{
    /// <summary>Requires a responsible, dated and reasoned decision tied to a known evaluation revision.</summary>
    /// <param name="validationContext">The context supplied by the boundary validating the review.</param>
    /// <returns>Violations of the manual-review contract.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EvaluationReferenceCode))
        {
            yield return new ValidationResult("A review requires an evaluation reference.", [nameof(EvaluationReferenceCode)]);
        }

        if (!Enum.IsDefined(State))
        {
            yield return new ValidationResult("Review state must be pending, enabled or not enabled.", [nameof(State)]);
            yield break;
        }

        if (EvaluationRevision is <= 0 || (State != RealTradingReviewState.Pending && EvaluationRevision is null))
        {
            yield return new ValidationResult("A recorded decision requires a known positive evaluation revision.", [nameof(EvaluationRevision)]);
        }

        if (State == RealTradingReviewState.Pending)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(ReviewerUserId))
        {
            yield return new ValidationResult("A recorded decision requires a reviewer.", [nameof(ReviewerUserId)]);
        }

        if (!ReviewedAtUtc.HasValue)
        {
            yield return new ValidationResult("A recorded decision requires its review date.", [nameof(ReviewedAtUtc)]);
        }

        if (string.IsNullOrWhiteSpace(Rationale))
        {
            yield return new ValidationResult("A recorded decision requires a rationale.", [nameof(Rationale)]);
        }
    }
}
