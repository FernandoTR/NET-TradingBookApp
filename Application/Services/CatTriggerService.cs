using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatTriggerService : ICatTriggerService
{
    private readonly IGenericRepository<CatTrigger> _repository;
    private readonly ICatTriggerRepository _catTriggerRepository;

    public CatTriggerService(IGenericRepository<CatTrigger> repository,ICatTriggerRepository catTriggerRepository)
    {
        _repository = repository;
        _catTriggerRepository = catTriggerRepository;
    }

    public async Task<bool> AddAsync(CatTrigger entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatTrigger>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatTrigger?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatTrigger entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    public async Task<List<GetTBAnalyticsTriggerDto>> GetTBAnalyticsTriggerAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catTriggerRepository.GetTBAnalyticsTriggerAsync(parameters);
    }
}
