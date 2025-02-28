using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Persistence.Repositories;

public class CatTriggerRepository : ICatTriggerRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public CatTriggerRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }
    /// <summary>
    ///  Obtiene una lista de Triggers para cargar un DataTable en la aplicación.
    /// </summary>
    /// <param name="parameters">Modelo con los parámetros necesarios para ejecutar el procedimiento almacenado.</param>
    /// <returns>Una lista de objetos <see cref="GetTBAnalyticsTriggerDto"/> correspondientes a los Triggers.</returns>
    public async Task<List<GetTBAnalyticsTriggerDto>> GetTBAnalyticsTriggerAsync(ParametersTBAnalyticsDto parameters)
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
                new SqlParameter("@DirectionId", SqlDbType.Int) { Value = parameters.DirectionId },

                new SqlParameter("@SearchValue", SqlDbType.NVarChar) { Value = parameters.SearchValue ?? (object)DBNull.Value },
                new SqlParameter("@OrderByColumn", SqlDbType.NVarChar) { Value = parameters.OrderByColumn ?? (object)DBNull.Value },
                new SqlParameter("@SortColumnDir", SqlDbType.NVarChar) { Value = parameters.SortColumnDir ?? (object)DBNull.Value },
                new SqlParameter("@Skip", SqlDbType.Int) { Value = parameters.Skip },
                new SqlParameter("@Take", SqlDbType.Int) { Value = parameters.Take },
                new SqlParameter("@Count", SqlDbType.Int) {  Direction = ParameterDirection.Output }

            };

            // Ejecutar el procedimiento almacenado y obtener los resultados
            var result = await _context.Set<GetTBAnalyticsTriggerDto>()
                .FromSqlRaw("EXEC usp_GetTBAnalyticsTrigger @CategoryId, @AccountTypeId, @InstrumentId, @FrameId,  @DirectionId, @SearchValue, @OrderByColumn, @SortColumnDir, @Skip, @Take, @Count OUTPUT", sqlParameters)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetTBAnalyticsTriggerAsync), ex);
            return new List<GetTBAnalyticsTriggerDto>();
        }

    }

    /// <summary>
    /// Obtiene la lista de efectividad por bloque para cargar en la grafica.
    /// </summary>
    /// <param name="parameters">Modelo con los parámetros necesarios para ejecutar el procedimiento almacenado.</param>
    /// <returns>Una lista de objetos <see cref="GetTBAnalyticsLastBlockDto"/> correspondientes a los ultimos bloques.</returns>
    public async Task<List<GetTBAnalyticsLastBlockDto>> GetTBAnalyticsLastBlockAsync(ParametersAnalyticsDto parameters)
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
                new SqlParameter("@DirectionId", SqlDbType.Int) { Value = parameters.DirectionId },
            };

            // Ejecutar el procedimiento almacenado y obtener los resultados
            var result = await _context.Set<GetTBAnalyticsLastBlockDto>()
                .FromSqlRaw("EXEC usp_GetTBAnalyticsLastBlock @CategoryId, @AccountTypeId, @InstrumentId, @FrameId, @DirectionId", sqlParameters)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetTBAnalyticsTriggerAsync), ex);
            return new List<GetTBAnalyticsLastBlockDto>();
        }

    }



}
