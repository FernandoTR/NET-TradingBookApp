using Application.DTOs;

namespace Application.Interfaces;

public interface ICatTimeRepository
{
    Task<List<GetTBAnalyticsTimeDto>> GetTBAnalyticsTimeAsync(ParametersTBAnalyticsDto parameters);
}
