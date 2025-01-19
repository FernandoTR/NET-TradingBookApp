using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatSceneryService : ICatSceneryService
{
    private readonly IGenericRepository<CatScenery> _repository;
    private readonly ICatSceneryRepository _catSceneryRepository;

    public CatSceneryService(IGenericRepository<CatScenery> repository, ICatSceneryRepository catSceneryRepository)
    {
        _repository = repository;
        _catSceneryRepository = catSceneryRepository;
    }

    public async Task<bool> AddAsync(CatScenery entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CatScenery>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CatScenery?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(CatScenery entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    public async Task<List<GetTBAnalyticsSceneryDto>> GetTBAnalyticsSceneryAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catSceneryRepository.GetTBAnalyticsSceneryAsync(parameters);
    }

}
