using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AccountBalancesService : IAccountBalancesService
{
    private readonly IGenericRepository<AccountBalance> _repository;
    private readonly IAccountBalanceRepository _accountBalanceRepository;
    public AccountBalancesService(IGenericRepository<AccountBalance> repository, IAccountBalanceRepository accountBalanceRepository)
    {
        _repository = repository;
        _accountBalanceRepository = accountBalanceRepository;
    }

    public Task<bool> AddAsync(AccountBalance entity)
    {
        return _repository.AddAsync(entity);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<AccountBalance>> GetAllAsync(QueryOptions<AccountBalance>? options = null)
    {
        return _repository.GetAllAsync(options);
    }

    public Task<AccountBalance?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(AccountBalance entity)
    {
        return _repository.UpdateAsync(entity);
    }

    public async Task<IEnumerable<AccountBalanceDto>> GetAllAccountBalanceByDateRangeAsync(string userId, DateTime dateStart, DateTime dateEnd)
    {
        return await _accountBalanceRepository.GetAllAccountBalanceByDateRangeAsync(userId, dateStart, dateEnd);
    }

    public async Task<AccountBalance?> GetByOrderIdAsync(int orderId)
    {
        return await _accountBalanceRepository.GetByOrderIdAsync(orderId);
    }

}
