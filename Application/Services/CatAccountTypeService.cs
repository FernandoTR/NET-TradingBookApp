using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatAccountTypeService : ICatAccountTypeService
{
    private readonly IGenericRepository<CatAccountType> _repository;
    public CatAccountTypeService(IGenericRepository<CatAccountType> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatAccountType entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatAccountType>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatAccountType?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatAccountType entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
