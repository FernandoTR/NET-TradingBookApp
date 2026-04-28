using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface ICatSceneryService
{
    Task<bool> AddAsync(CatScenery entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatScenery>> GetAllAsync();
    Task<CatScenery?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatScenery entity);
    Task<List<GetTBAnalyticsSceneryDto>> GetTBAnalyticsSceneryAsync(ParametersTBAnalyticsDto parameters);
    Task<List<GetTBAnalyticsSceneryDto>> GetTBAnalyticsSceneryAutoScoreAsync(ParametersTBAnalyticsDto parameters);    
}
