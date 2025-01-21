using Application.Common;
using Infrastructure;

namespace Application.Interfaces;

public interface IAccountsService
{
    Task<bool> AddAsync(Account entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Account>> GetAllAsync(QueryOptions<Account>? options = null);
    Task<Account?> GetByIdAsync(int id, QueryOptions<Account>? options = null);
    Task<bool> UpdateAsync(Account entity);
    Task<bool> WithDrawCashAsync(AccountBalance model);
    Task<bool> AddCashAsync(AccountBalance model);
}
