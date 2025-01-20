using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatStageService : ICatStageService
{
    private readonly IGenericRepository<CatStage> _repository;
    private readonly ICatStageRepository _catStageRepository;

    public CatStageService(IGenericRepository<CatStage> repository, ICatStageRepository catStageRepository)
    {
        _repository = repository;
        _catStageRepository = catStageRepository;
    }

    public async Task<bool> AddAsync(CatStage entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatStage>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatStage?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatStage entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    public async Task<List<GetTBAnalyticsStageDto>> GetTBAnalyticsStageAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catStageRepository.GetTBAnalyticsStageAsync(parameters);
    }



}
