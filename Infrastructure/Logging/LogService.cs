using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Logging;

public class LogService : ILogService
{
    private readonly ApplicationDbContext _dbContext;

    public LogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Inserta logs de error en base de datos.
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="exception"></param>
    public void ErrorLog(string methodName, Exception exception)
    {
        try
        {
            var extras = string.Empty;

            if (exception is DbUpdateException dbUpdateException)
            {
                foreach (var entry in dbUpdateException.Entries)
                {
                    extras += $"Entity: {entry.Entity.GetType().Name}, State: {entry.State} ";
                }
            }
            else if (exception is ValidationException validationException)
            {
                extras = validationException.Message;
            }

            var log = new ErrorLog
            {
                MethodName = methodName,
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = exception.StackTrace + ":" + extras,
                ExceptionString = exception.ToString(),
                LogDate = DateTime.UtcNow,
                ApplicationId = (int)Domain.Enums.Application.WebAppBase
            };

            _dbContext.ErrorLogs.Add(log);
            _dbContext.SaveChanges();
        }
        catch
        {
            // Se sugiere loggear este error en un logger externo
        }
    }

    /// <summary>
    /// Inserta logs de error en base de datos sin ser producidos por exception.
    /// </summary>
    public void ErrorLog(string methodName, string message, string details)
    {
        try
        {
            var log = new ErrorLog
            {
                MethodName = methodName,
                ExceptionMessage = message,
                ExceptionStackTrace = "No se creó por excepción",
                ExceptionString = details,
                LogDate = DateTime.UtcNow,
                ApplicationId = (int)Domain.Enums.Application.WebAppBase
            };

            _dbContext.ErrorLogs.Add(log);
            _dbContext.SaveChanges();
        }
        catch
        {
            // Se sugiere loggear este error en un logger externo
        }
    }

    /// <summary>
    /// Inserta logs de actividad en base de datos.
    /// </summary>
    public void ActivityLog(string userId, string eventType, string description)
    {
        try
        {
            var log = new ActivityLog
            {
                LogDate = DateTime.UtcNow,
                EventType = eventType,
                Description = description,
                UserId = userId,
                ApplicationId = (int)Domain.Enums.Application.WebAppBase
            };

            _dbContext.ActivityLogs.Add(log);
            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            ErrorLog("LogService.ActivityLog", ex);
            throw;
        }
    }
}
