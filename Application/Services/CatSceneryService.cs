using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatSceneryService : ICatSceneryService
{
    private readonly IGenericRepository<CatScenery> _repository;
    public CatSceneryService(IGenericRepository<CatScenery> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatScenery entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatScenery>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatScenery?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatScenery entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
