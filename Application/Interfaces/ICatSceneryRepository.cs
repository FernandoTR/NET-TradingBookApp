using Application.DTOs;

namespace Application.Interfaces;

public interface ICatSceneryRepository
{
    Task<List<GetTBAnalyticsSceneryDto>> GetTBAnalyticsSceneryAsync(ParametersTBAnalyticsDto parameters);
}
