using Infrastructure;

namespace Application.Interfaces;

public interface IActivityLogService
{
    Task<IEnumerable<ActivityLog>> GetAllLogsByDateRangeAsync(DateTime dateStart, DateTime dateEnd);
}
