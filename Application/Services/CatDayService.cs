using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatDayService : ICatDayService
{
    private readonly IGenericRepository<CatDay> _repository;
    private readonly ICatDayRepository _catDayRepository;

    public CatDayService(IGenericRepository<CatDay> repository, ICatDayRepository catDayRepository)
    {
        _repository = repository;
        _catDayRepository = catDayRepository;
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

    public async Task<List<GetTBAnalyticsDayDto>> GetTBAnalyticsDayAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catDayRepository.GetTBAnalyticsDayAsync(parameters);
    }
}
