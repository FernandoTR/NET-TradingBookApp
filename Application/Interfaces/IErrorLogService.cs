using Infrastructure;

namespace Application.Interfaces;

public interface IErrorLogService
{
    Task<IEnumerable<ErrorLog>> GetAllByDateRangeAsync(DateTime dateStart, DateTime dateEnd);
    Task<ErrorLog?> GetByIdAsync(long id);
}
