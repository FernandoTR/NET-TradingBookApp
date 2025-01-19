
using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface ICatFigureService
{
    Task<bool> AddAsync(CatFigure entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatFigure>> GetAllAsync();
    Task<CatFigure?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatFigure entity);
    Task<List<GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(ParametersTBAnalyticsDto parameters);
}
