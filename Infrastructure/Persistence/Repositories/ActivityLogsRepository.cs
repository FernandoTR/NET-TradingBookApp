using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ActivityLogsRepository : IActivityLogsRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public ActivityLogsRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    /// <summary>
    /// Obtiene todos los registros de actividad dentro de un rango de fechas específico.
    /// </summary>
    /// <param name="dateStart">Fecha de inicio del rango.</param>
    /// <param name="dateEnd">Fecha de fin del rango.</param>
    /// <returns>Una colección de registros de actividad encontrados dentro del rango de fechas.</returns>
    /// <exception cref="ArgumentException">Se lanza si la fecha de inicio es mayor que la fecha de fin.</exception>
    public async Task<IEnumerable<ActivityLog>> GetAllLogsByDateRangeAsync(DateTime dateStart, DateTime dateEnd)
    {
        if (dateStart > dateEnd)
            throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha de fin.", nameof(dateStart));

        try
        {
            // Filtra los registros por la aplicación y el rango de fechas.
            return await (from activityLog in _context.ActivityLogs
                          join user in _context.Users on activityLog.UserId equals user.Id
                          where activityLog.ApplicationId == (int)Domain.Enums.Application.WebAppBase
                                && activityLog.LogDate >= dateStart
                                && activityLog.LogDate <= dateEnd
                          select new ActivityLog
                          {
                              Id = activityLog.Id,
                              LogDate = activityLog.LogDate,
                              EventType = activityLog.EventType,
                              Description = activityLog.Description,
                              UserId = activityLog.UserId,
                              ApplicationId = activityLog.ApplicationId,
                              User = new AspNetUser
                              {
                                  Id = user.Id,
                                  UserName = user.UserName
                              }
                          }).ToListAsync();
            //return await _context.ActivityLogs
            //                     .Where(log => log.ApplicationId == (int)Domain.Enums.Application.WebAppBase
            //                                   && log.LogDate >= dateStart && log.LogDate <= dateEnd)
            //                     .ToListAsync();
        }
        catch (Exception ex)
        {
            // Registra el error utilizando el servicio de registro.
            _logService.ErrorLog(nameof(GetAllLogsByDateRangeAsync), ex);

            // Retorna una colección vacía en caso de error.
            return Enumerable.Empty<ActivityLog>();
        }
    }
}
