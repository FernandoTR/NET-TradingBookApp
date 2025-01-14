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

    public Task<bool> AddAsync(CatInstrument entity)
    {
        return _repository.AddAsync(entity);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<CatInstrument>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<CatInstrument?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(CatInstrument entity)
    {
        return _repository.UpdateAsync(entity);
    }
}
