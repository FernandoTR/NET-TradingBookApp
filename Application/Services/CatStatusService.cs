using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatStatusService : ICatStatusService
{
    private readonly IGenericRepository<CatStatus> _repository;
    public CatStatusService(IGenericRepository<CatStatus> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatStatus entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatStatus>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatStatus?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatStatus entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
