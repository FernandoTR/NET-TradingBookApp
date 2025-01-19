using Infrastructure;

namespace Application.Interfaces;

public interface ICatStatusService
{
    Task<bool> AddAsync(CatStatus entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatStatus>> GetAllAsync();
    Task<CatStatus?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatStatus entity);
}
