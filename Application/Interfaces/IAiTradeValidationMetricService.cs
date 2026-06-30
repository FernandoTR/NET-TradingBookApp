using Application.DTOs.AiValidation;

namespace Application.Interfaces;

public interface IAiTradeValidationMetricService
{
    Task CreateInitialMetricAsync(int validationId, string userId, CancellationToken cancellationToken);

    Task RefreshOrderOutcomeAsync(int orderId, CancellationToken cancellationToken);

    Task<AiValidationMetricSummaryDto> GetSummaryAsync(AiValidationMetricFilterDto filter, CancellationToken cancellationToken);
}
