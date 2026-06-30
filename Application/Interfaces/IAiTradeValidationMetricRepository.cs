using Application.DTOs.AiValidation;
using Infrastructure;

namespace Application.Interfaces;

public interface IAiTradeValidationMetricRepository
{
    Task<AiTradeValidation?> GetValidationForMetricAsync(int validationId, string userId, CancellationToken cancellationToken);

    Task<AiTradeValidationMetric?> GetByValidationIdAsync(int validationId, CancellationToken cancellationToken);

    Task<AiTradeValidationMetric?> GetByLinkedOrderIdAsync(int orderId, CancellationToken cancellationToken);

    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AiTradeValidationMetric>> GetForSummaryAsync(AiValidationMetricFilterDto filter, CancellationToken cancellationToken);

    Task AddAsync(AiTradeValidationMetric metric, CancellationToken cancellationToken);

    Task UpdateAsync(AiTradeValidationMetric metric, CancellationToken cancellationToken);
}
