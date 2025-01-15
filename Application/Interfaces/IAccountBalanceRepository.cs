using Application.DTOs;

namespace Application.Interfaces;

public interface IAccountBalanceRepository
{
    Task<IEnumerable<AccountBalanceDto>> GetAllAccountBalanceByDateRangeAsync(string userId, DateTime dateStart, DateTime dateEnd);
}
