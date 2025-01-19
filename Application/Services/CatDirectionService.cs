using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatDirectionService : ICatDirectionService
{
    private readonly IGenericRepository<CatDirection> _repository;
    public CatDirectionService(IGenericRepository<CatDirection> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatDirection entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatDirection>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatDirection?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatDirection entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
