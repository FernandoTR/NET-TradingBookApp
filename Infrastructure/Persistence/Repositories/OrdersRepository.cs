using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Persistence.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public OrdersRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    /// <summary>
    ///  Obtiene el listado para cargar el dataTable de "Ordenes" dentro de la aplicación.
    /// </summary>
    /// <param name="parameters">Modelo con los parámetros necesarios para ejecutar el procedimiento almacenado.</param>
    /// <returns>Una lista de objetos <see cref="GetOrdersDataTableDto"/> correspondientes a las ultimas ordenes.</returns>
    public async Task<List<GetOrdersDataTableDto>> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters)
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
               
                new SqlParameter("@Skip", SqlDbType.Int) { Value = parameters.Skip },
                new SqlParameter("@Take", SqlDbType.Int) { Value = parameters.Take },
                new SqlParameter("@Count", SqlDbType.Int) {  Direction = ParameterDirection.Output }

            };

            // Ejecutar el procedimiento almacenado y obtener los resultados
            var result = await _context.Set<GetOrdersDataTableDto>()
                .FromSqlRaw("EXEC usp_GetOrdersDataTable @CategoryId, @AccountTypeId, @InstrumentId, @FrameId, @Skip, @Take, @Count OUTPUT", sqlParameters)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetOrdersDataTableAsync), ex);
            return new List<GetOrdersDataTableDto>();
        }

    }
}
