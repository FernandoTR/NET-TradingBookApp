using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CanFigureService : ICanFigureService
{
    private readonly IGenericRepository<CatFigure> _repository;
    public CanFigureService(IGenericRepository<CatFigure> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(CatFigure entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatFigure>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatFigure?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatFigure entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
