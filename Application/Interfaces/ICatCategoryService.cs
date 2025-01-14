using Infrastructure;

namespace Application.Interfaces;

public interface ICatCategoryService
{
    Task<bool> AddAsync(CatCategory entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatCategory>> GetAllAsync();
    Task<CatCategory?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatCategory entity);
}
