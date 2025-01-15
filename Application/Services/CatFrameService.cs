using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatFrameService : ICatFrameService
{
    private readonly IGenericRepository<CatFrame> _repository;
    public CatFrameService(IGenericRepository<CatFrame> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatFrame entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatFrame>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatFrame?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatFrame entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
