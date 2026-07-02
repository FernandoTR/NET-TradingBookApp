using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly IErrorLogRepository _repository;

    public ErrorLogService(IErrorLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ErrorLog>> GetAllByDateRangeAsync(DateTime dateStart, DateTime dateEnd)
    {
        return await _repository.GetAllByDateRangeAsync(dateStart, dateEnd);
    }

    public async Task<ErrorLog?> GetByIdAsync(long id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
