using Infrastructure;

namespace Application.Interfaces;

public interface ICatDirectionService
{
    Task<bool> AddAsync(CatDirection entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatDirection>> GetAllAsync();
    Task<CatDirection?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatDirection entity);
}
