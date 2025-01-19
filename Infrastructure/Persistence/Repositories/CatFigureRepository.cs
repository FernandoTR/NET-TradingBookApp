using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Persistence.Repositories;

public class CatFigureRepository : ICatFigureRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public CatFigureRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }


    /// <summary>
    ///  Obtiene una lista de figuras para cargar un DataTable en la aplicación.
    /// </summary>
    /// <param name="parameters">Modelo con los parámetros necesarios para ejecutar el procedimiento almacenado.</param>
    /// <returns>Una lista de objetos <see cref="GetTBAnalyticsFigureDto"/> correspondientes a las figuras.</returns>
    public async Task<List<GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(ParametersTBAnalyticsDto parameters)
    {
        try
        {
            // Crear los parámetros para el procedimiento almacenado
            var sqlParameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = parameters.CategoryId },
                new SqlParameter("@AccountTypeId", SqlDbType.Int) { Value = parameters.AccountTypeId },
                new SqlParameter("@InstrumentId", SqlDbType.Int) { Value = parameters.InstrumentId },
                new SqlParameter("@FrameId", SqlDbType.Int) { Value = parameters.FrameId },
                new SqlParameter("@SearchValue", SqlDbType.NVarChar) { Value = parameters.SearchValue ?? (object)DBNull.Value },
                new SqlParameter("@OrderByColumn", SqlDbType.NVarChar) { Value = parameters.OrderByColumn ?? (object)DBNull.Value },
                new SqlParameter("@SortColumnDir", SqlDbType.NVarChar) { Value = parameters.SortColumnDir ?? (object)DBNull.Value },
                new SqlParameter("@Skip", SqlDbType.Int) { Value = parameters.Skip },
                new SqlParameter("@Take", SqlDbType.Int) { Value = parameters.Take },
                new SqlParameter("@Count", SqlDbType.Int) {  Direction = ParameterDirection.Output }

            };

            // Ejecutar el procedimiento almacenado y obtener los resultados
            var result = await _context.Set<GetTBAnalyticsFigureDto>()
                .FromSqlRaw("EXEC usp_GetTBAnalyticsFigure @CategoryId, @AccountTypeId, @InstrumentId, @FrameId, @SearchValue, @OrderByColumn, @SortColumnDir, @Skip, @Take, @Count OUTPUT", sqlParameters)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetTBAnalyticsFigureAsync), ex);
            return new List<GetTBAnalyticsFigureDto>();
        }

    }
}
