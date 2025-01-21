using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly ICatTimeRepository _catTimeRepository;

    public OrdersService(IOrdersRepository ordersRepository, ICatTimeRepository catTimeRepository)
    {
        _ordersRepository = ordersRepository;
        _catTimeRepository = catTimeRepository;
    }

    public async Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _ordersRepository.GetOrdersDataTableAsync(parameters);
    }

    public async Task<List<GetTBAnalyticsTimeDto>> GetTBAnalyticsTimeAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catTimeRepository.GetTBAnalyticsTimeAsync(parameters);
    }

}
