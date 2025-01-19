using Application.DTOs;

namespace Application.Interfaces;

public interface ICatFigureRepository
{
    Task<List<GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(ParametersTBAnalyticsDto parameters);
}
