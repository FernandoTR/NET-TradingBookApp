using Application.Common;

namespace Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, QueryOptions<T>? options = null);
    Task<IEnumerable<T>> GetAllAsync(QueryOptions<T>? options = null);
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
