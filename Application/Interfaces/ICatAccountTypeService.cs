using Infrastructure;

namespace Application.Interfaces;

public interface ICatAccountTypeService
{   
    Task<bool> AddAsync(CatAccountType entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatAccountType>> GetAllAsync();
    Task<CatAccountType?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatAccountType entity);
}
