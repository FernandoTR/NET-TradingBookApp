using Application.DTOs;

namespace Application.Interfaces;

public interface IOrdersService
{
    Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters);
    Task<List<GetTBAnalyticsTimeDto>> GetTBAnalyticsTimeAsync(ParametersTBAnalyticsDto parameters);
}
