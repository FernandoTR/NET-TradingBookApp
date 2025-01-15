using Application.Common;
using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface IAccountBalancesService
{
    Task<bool> AddAsync(AccountBalance entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<AccountBalance>> GetAllAsync(QueryOptions<AccountBalance>? options = null);
    Task<AccountBalance?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(AccountBalance entity);
    Task<IEnumerable<AccountBalanceDto>> GetAllAccountBalanceByDateRangeAsync(string userId, DateTime dateStart, DateTime dateEnd);
}
