using Application.DTOs;
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
}
