using Application.DTOs;

namespace Application.Interfaces;

public interface ICatDayRepository
{
    Task<List<GetTBAnalyticsDayDto>> GetTBAnalyticsDayAsync(ParametersTBAnalyticsDto parameters);
}
