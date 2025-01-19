using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatDayService : ICatDayService
{
    private readonly IGenericRepository<CatDay> _repository;
    public CatDayService(IGenericRepository<CatDay> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatDay entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatDay>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatDay?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatDay entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
