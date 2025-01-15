using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatCategoryService : ICatCategoryService
{
    private readonly IGenericRepository<CatCategory> _repository;
    public CatCategoryService(IGenericRepository<CatCategory> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatCategory entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatCategory>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatCategory?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatCategory entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
