using Application.DTOs;

namespace Application.Interfaces;

public interface ICatTriggerRepository
{
    Task<List<GetTBAnalyticsTriggerDto>> GetTBAnalyticsTriggerAsync(ParametersTBAnalyticsDto parameters);
}
