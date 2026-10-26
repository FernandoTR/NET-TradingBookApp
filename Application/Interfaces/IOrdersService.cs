using Application.DTOs;
using Application.DTOs.AiValidation.V2;
using Infrastructure;

namespace Application.Interfaces;

public interface IOrdersService
{
    Task<bool> AddAsync(Order entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(Order entity);
    Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters);
    Task<List<GetTBAnalyticsTimeDto>> GetTBAnalyticsTimeAsync(ParametersTBAnalyticsDto parameters);
    Task<(bool, int)> AddOrderAsync(Order entity, Trade trade);
    Task<bool> DeleteOrderAsync(int id);
    Task<bool> CloseOrderAsync(Order entity, Trade trade);
    Task<bool> DownloadImageTradingViewAsync();

    /// <summary>Calculates a new contextual evaluation while retaining the order's original score and manual Scenery.</summary>
    /// <param name="entity">The source order, which is not mutated or saved by this calculation.</param>
    /// <param name="input">The explicit contextual evidence for the new evaluation.</param>
    /// <returns>The order linkage, original score and independently versioned new calculation.</returns>
    OrderContextualScoreEvaluationDto ReevaluateContext(Order entity, ContextualScoreInputDto input);

    /// <summary>Adapts recorded order data for an explicit contextual recalculation, keeping missing evidence unknown.</summary>
    /// <param name="entity">The source order, which is not mutated or saved by this calculation.</param>
    /// <param name="cancellationToken">Cancellation for catalog lookups.</param>
    /// <returns>The original score and the separately identified contextual evaluation.</returns>
    Task<OrderContextualScoreEvaluationDto> ReevaluateContextAsync(Order entity, CancellationToken cancellationToken);
}
