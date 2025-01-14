using Infrastructure;

namespace Application.Interfaces;

public interface ICatFrameService
{
    Task<bool> AddAsync(CatFrame entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatFrame>> GetAllAsync();
    Task<CatFrame?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatFrame entity);
}
