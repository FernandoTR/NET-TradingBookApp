using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatInstrumentsService : ICatInstrumentsService
{
    private readonly IGenericRepository<CatInstrument> _repository;
    public CatInstrumentsService(IGenericRepository<CatInstrument> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatInstrument entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatInstrument>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatInstrument?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatInstrument entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
