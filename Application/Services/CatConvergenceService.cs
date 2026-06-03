using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class CatConvergenceService : ICatConvergenceService
{
    private readonly ICatConvergenceRepository _catConvergenceRepository;

    public CatConvergenceService(ICatConvergenceRepository catConvergenceRepository)
    {
        _catConvergenceRepository = catConvergenceRepository;
    }

    public async Task<(List<GetTBAnalyticsConvergenceDto> data, int totalCount)> GetTBAnalyticsConvergenceAsync(ParametersTBAnalyticsConvergenceDto parameters)
    {
        return await _catConvergenceRepository.GetTBAnalyticsConvergenceAsync(parameters);
    }
}
