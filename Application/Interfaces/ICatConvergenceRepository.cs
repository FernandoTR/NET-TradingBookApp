using Application.DTOs;

namespace Application.Interfaces;

public interface ICatConvergenceRepository
{
    Task<(List<GetTBAnalyticsConvergenceDto> data, int totalCount)> GetTBAnalyticsConvergenceAsync(ParametersTBAnalyticsConvergenceDto parameters);
}
