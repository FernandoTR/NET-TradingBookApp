using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Infrastructure;

namespace Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IGenericRepository<Order> _repository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly ICatTimeRepository _catTimeRepository;
    private readonly ITradesService _tradesService;
    private readonly ILogService _logService;
    private readonly IAccountBalancesService _accountBalancesService;
    private readonly IAccountsService _accountsService;
    public OrdersService(IGenericRepository<Order> repository,
                         IOrdersRepository ordersRepository,
                         ICatTimeRepository catTimeRepository,
                         ITradesService tradesService,
                         ILogService logService,
                         IAccountBalancesService accountBalancesService,
                         IAccountsService accountsService)
    {
        _repository = repository;
        _ordersRepository = ordersRepository;
        _catTimeRepository = catTimeRepository;
        _tradesService = tradesService;
        _logService = logService;
        _accountBalancesService = accountBalancesService;
        _accountsService = accountsService;
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

    public async Task<bool> DeleteOrderAsync(int id)
    {
        // Recuperar el trade asociado a la orden
        var options = new QueryOptions<Trade>
        {
            Where = x => x.OrderId == id
        };

        var trade = (await _tradesService.GetAllAsync(options)).FirstOrDefault();
        if (trade == null)
        {
            return false;
        }

        var resultTrade = await _tradesService.DeleteAsync(trade.Id);
        if (!resultTrade)
        {
            return false;
        }

        // Recuperar de la tabla AccountBalances la acción que se realizó en la orden
        var accountBalance = await _accountBalancesService.GetByOrderIdAsync(id);
        if (accountBalance != null)
        {
            var balance = new AccountBalance
            {
                AccountId = accountBalance.AccountId,
                OrderId = null,
                Balance = Math.Abs(accountBalance.Balance),
                Reference = $"Cancelación de orden #{id}, {accountBalance.Reference}"
            };


            await _accountBalancesService.DeleteAsync(accountBalance.Id);

            // Descuenta el monto de la cuenta seleccionada
            await _accountsService.AddCashAsync(balance);
        }


        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> CloseOrderAsync(Order entity, Trade trade)
    {
        return await _ordersRepository.CloseOrderAsync(entity, trade);
    }

}
