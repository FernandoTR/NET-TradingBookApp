using Infrastructure;

namespace Application.Interfaces;

public interface ICatDayService
{
    Task<bool> AddAsync(CatDay entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatDay>> GetAllAsync();
    Task<CatDay?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatDay entity);
}
