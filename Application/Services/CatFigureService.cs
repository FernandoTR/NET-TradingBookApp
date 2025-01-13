

using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatFigureService : ICatFigureService
{
    private readonly IGenericRepository<CatFigure> _repository;
    public CatFigureService(IGenericRepository<CatFigure> repository)
    {
        _repository = repository;
    }

    public Task<bool> AddAsync(CatFigure entity)
    {
        return _repository.AddAsync(entity);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<CatFigure>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<CatFigure?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(CatFigure entity)
    {
        return _repository.UpdateAsync(entity);
    }
}
