using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ErrorLogRepository : IErrorLogRepository
{
    private readonly LoggingDbContext _context;
    private readonly ILogService _logService;

    public ErrorLogRepository(LoggingDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IEnumerable<ErrorLog>> GetAllByDateRangeAsync(DateTime dateStart, DateTime dateEnd)
    {
        if (dateStart > dateEnd)
            throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha de fin.", nameof(dateStart));

        try
        {
            return await _context.ErrorLogs
                .AsNoTracking()
                .Where(log => log.ApplicationId == (int)Domain.Enums.Application.WebAppBase
                    && log.LogDate >= dateStart
                    && log.LogDate <= dateEnd)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllByDateRangeAsync), ex);
            return Enumerable.Empty<ErrorLog>();
        }
    }

    public async Task<ErrorLog?> GetByIdAsync(long id)
    {
        try
        {
            return await _context.ErrorLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(log => log.Id == id
                    && log.ApplicationId == (int)Domain.Enums.Application.WebAppBase);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetByIdAsync), ex);
            return null;
        }
    }
}
