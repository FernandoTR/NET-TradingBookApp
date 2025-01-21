using Infrastructure;

namespace Application.Interfaces;

public interface ITradesService
{
    Task<bool> AddAsync(Trade entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Trade>> GetAllAsync();
    Task<Trade?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(Trade entity);
}
