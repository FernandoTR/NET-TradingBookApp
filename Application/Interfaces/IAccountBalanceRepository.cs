using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface IAccountBalanceRepository
{
    Task<IEnumerable<AccountBalanceDto>> GetAllAccountBalanceByDateRangeAsync(string userId, DateTime dateStart, DateTime dateEnd);
    Task<AccountBalance?> GetByOrderIdAsync(int orderId);
}
