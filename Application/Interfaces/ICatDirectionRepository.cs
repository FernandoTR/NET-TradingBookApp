using Application.DTOs;

namespace Application.Interfaces;

public interface ICatDirectionRepository
{
    Task<List<GetTBAnalyticsDirectionDto>> GetTBAnalyticsDirectionAsync(ParametersTBAnalyticsDto parameters);
}
