using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context, ILogService logService)
    {        
        _context = context;
        _dbSet = _context.Set<T>();
        _logService = logService;
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<bool> AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(AddAsync), ex);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(DeleteAsync), ex);
            return false;
        }
    }
}
