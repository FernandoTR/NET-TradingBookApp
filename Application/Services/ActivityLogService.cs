

using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogsRepository _repository;

    public ActivityLogService(IActivityLogsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ActivityLog>> GetAllLogsByDateRangeAsync(DateTime dateStart, DateTime dateEnd)
    {
        return await _repository.GetAllLogsByDateRangeAsync(dateStart, dateEnd);
    }
}
