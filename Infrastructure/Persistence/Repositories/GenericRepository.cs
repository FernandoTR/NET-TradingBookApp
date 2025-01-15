using Application.Common;
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

    public async Task<T?> GetByIdAsync(int id, QueryOptions<T>? options = null)
    {
        // Inicia la consulta con el DbSet correspondiente
        IQueryable<T> query = _dbSet;

        // Aplica las opciones de filtrado, orden y relaciones si se proporcionan
        if (options != null)
        {
            if (options.HasWhere)
            {
                query = query.Where(options.Where);
            }

            if (options.HasOrderBy)
            {
                query = query.OrderBy(options.OrderBy);
            }

            foreach (var include in options.GetIncludes())
            {
                query = query.Include(include);
            }
        }

        // Obtiene el nombre de la clave primaria
        var primaryKey = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
        if (primaryKey == null)
        {
            throw new InvalidOperationException($"No se encontró clave primaria para la entidad {typeof(T).Name}.");
        }

        var primaryKeyName = primaryKey.Properties.FirstOrDefault()?.Name;
        if (string.IsNullOrEmpty(primaryKeyName))
        {
            throw new InvalidOperationException($"La clave primaria para la entidad {typeof(T).Name} no tiene un nombre válido.");
        }

        // Busca la entidad utilizando la clave primaria
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKeyName) == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync(QueryOptions<T>? options = null)
    {
        // Inicia la consulta con el DbSet correspondiente
        IQueryable<T> query = _dbSet;

        // Aplica las opciones de filtrado, orden y relaciones si se proporcionan
        if (options != null)
        {
            if (options.HasWhere)
            {
                query = query.Where(options.Where);
            }

            if (options.HasOrderBy)
            {
                query = query.OrderBy(options.OrderBy);
            }

            foreach (var include in options.GetIncludes())
            {
                query = query.Include(include);
            }
        }        

        return await query.ToListAsync();
    }

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
