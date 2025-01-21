using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IGenericRepository<Order> _repository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly ICatTimeRepository _catTimeRepository;
    private readonly ITradesService _tradesService;
    private readonly ILogService _logService;

    public OrdersService(IGenericRepository<Order> repository,
                         IOrdersRepository ordersRepository,
                         ICatTimeRepository catTimeRepository,
                         ITradesService tradesService,
                         ILogService logService)
    {
        _repository = repository;
        _ordersRepository = ordersRepository;
        _catTimeRepository = catTimeRepository;
        _tradesService = tradesService;
        _logService = logService;
    }

    public async Task<bool> AddAsync(Order entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(Order entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    public async Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _ordersRepository.GetOrdersDataTableAsync(parameters);
    }

    public async Task<List<GetTBAnalyticsTimeDto>> GetTBAnalyticsTimeAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catTimeRepository.GetTBAnalyticsTimeAsync(parameters);
    }

    public async Task<(bool, int)> AddOrderAsync(Order entity, Trade trade)
    {
        return await _ordersRepository.AddOrderAsync(entity, trade);
    }


}
