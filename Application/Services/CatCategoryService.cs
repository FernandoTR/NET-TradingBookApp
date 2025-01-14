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

    public Task<bool> AddAsync(CatCategory entity)
    {
        return _repository.AddAsync(entity);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<CatCategory>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<CatCategory?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(CatCategory entity)
    {
        return _repository.UpdateAsync(entity);
    }
}
