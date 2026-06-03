using Application.DTOs;

namespace Application.Interfaces;

public interface ICatConvergenceService
{
    Task<(List<GetTBAnalyticsConvergenceDto> data, int totalCount)> GetTBAnalyticsConvergenceAsync(ParametersTBAnalyticsConvergenceDto parameters);
}
