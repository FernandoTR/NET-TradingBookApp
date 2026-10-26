using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;

namespace Application.Interfaces;

public interface IAiTradeValidationMetricService
{
    Task CreateInitialMetricAsync(int validationId, string userId, CancellationToken cancellationToken);

    Task RefreshOrderOutcomeAsync(int orderId, CancellationToken cancellationToken);

    Task<AiValidationMetricSummaryDto> GetSummaryAsync(string userId, AiValidationMetricFilterDto filter, CancellationToken cancellationToken);

    /// <summary>Gets V2 metrics for one authenticated user and denominator/version-comparable snapshot populations.</summary>
    Task<AiValidationMetricSummaryV2Dto> GetSummaryV2Async(
        string userId, AiValidationMetricFilterV2Dto filter, CancellationToken cancellationToken);
}
