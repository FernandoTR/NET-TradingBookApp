using Application.DTOs;

namespace Application.Interfaces;

public interface IOrdersRepository
{
    Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters);
}
