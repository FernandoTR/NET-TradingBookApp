using Application.DTOs;

namespace Application.Interfaces;

public interface ICatTriggerRepository
{
    Task<List<GetTBAnalyticsTriggerDto>> GetTBAnalyticsTriggerAsync(ParametersTBAnalyticsDto parameters);

    Task<List<GetTBAnalyticsLastBlockDto>> GetTBAnalyticsLastBlockAsync(ParametersAnalyticsDto parameters);
}
