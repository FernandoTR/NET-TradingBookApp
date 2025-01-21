using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface IOrdersRepository
{
    Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters);
    Task<(bool, int)> AddOrderAsync(Order entity, Trade trade);
}
