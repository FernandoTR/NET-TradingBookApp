

using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatFigureService : ICatFigureService
{
    private readonly IGenericRepository<CatFigure> _repository;
    private readonly ICatFigureRepository _catFigureRepository;

    public CatFigureService(IGenericRepository<CatFigure> repository, ICatFigureRepository catFigureRepository)
    {
        _repository = repository;
        _catFigureRepository = catFigureRepository;
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

    public async Task<List<GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catFigureRepository.GetTBAnalyticsFigureAsync(parameters);
    }


}
