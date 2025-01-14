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

    public Task<bool> AddAsync(CatFrame entity)
    {
        return _repository.AddAsync(entity);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<CatFrame>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<CatFrame?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(CatFrame entity)
    {
        return _repository.UpdateAsync(entity);
    }
}
