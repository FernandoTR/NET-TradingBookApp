using Application.DTOs;

namespace Application.Interfaces;

public interface ICatStageRepository
{
    Task<List<GetTBAnalyticsStageDto>> GetTBAnalyticsStageAsync(ParametersTBAnalyticsDto parameters);
}
