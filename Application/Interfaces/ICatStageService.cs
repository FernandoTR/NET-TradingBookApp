using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface ICatStageService
{
    Task<bool> AddAsync(CatStage entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatStage>> GetAllAsync();
    Task<CatStage?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatStage entity);
    Task<List<GetTBAnalyticsStageDto>> GetTBAnalyticsStageAsync(ParametersTBAnalyticsDto parameters);
}
