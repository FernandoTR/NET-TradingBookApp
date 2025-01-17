using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface ICatTriggerService
{
    Task<bool> AddAsync(CatTrigger entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatTrigger>> GetAllAsync();
    Task<CatTrigger?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatTrigger entity);
    Task<List<GetTBAnalyticsTriggerDto>> GetTBAnalyticsTriggerAsync(ParametersTBAnalyticsDto parameters);
}
