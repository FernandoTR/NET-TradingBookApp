using Infrastructure;

namespace Application.Interfaces;

public interface IErrorLogRepository
{
    Task<IEnumerable<ErrorLog>> GetAllByDateRangeAsync(DateTime dateStart, DateTime dateEnd);
    Task<ErrorLog?> GetByIdAsync(long id);
}
