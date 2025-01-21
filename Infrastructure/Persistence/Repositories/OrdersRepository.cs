using Application.DTOs;
using Application.Interfaces;
using Application.Services;
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
    public async Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters)
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

            // Recuperar el valor del parámetro de salida después de la ejecución
            int totalCount = (int)(sqlParameters[6].Value ?? 0);

            return (result, totalCount);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetOrdersDataTableAsync), ex);
            return (new List<GetOrdersDataTableDto>(),0);
        }

    }

    public async Task<(bool, int)> AddOrderAsync(Order entity, Trade trade)
    {        
        try
        {
            int id = 0;

            // Iniciar una transacción para asegurar atomicidad.
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Agregar el pedido (Order) a la base de datos.
            await _context.Orders.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Asociar el ID de la orden a la transacción (Trade).
            trade.OrderId = entity.Id;
            id = entity.Id;

            // Agregar la transacción (Trade) al servicio correspondiente.
            var tradeResult = await _context.Trades.AddAsync(trade);

            // Guarda los cambios y confirma la transacción si es exitoso
            var changesSaved = await _context.SaveChangesAsync() > 0;

            if (changesSaved)
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync(); // Revertir cambios si falla.
            }

            return (changesSaved, id);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog("OrdersService: AddAsync", ex);
            throw;
        }
    }



}
