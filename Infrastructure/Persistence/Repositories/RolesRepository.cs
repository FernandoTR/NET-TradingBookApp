using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Persistence.Repositories;

public class RolesRepository : IRolesRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public RolesRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    /// <summary>
    /// Agrega un rol y su lista asociada de menús de acceso de manera asincrónica.
    /// </summary>
    /// <param name="role">El rol que se agregará.</param>
    /// <param name="accessMenus">La lista de menús de acceso relacionados con el rol.</param>
    /// <returns>Un valor booleano que indica si la operación fue exitosa.</returns>
    public async Task<bool> AddAsync(AspNetRole role, List<AccessMenu> accessMenus)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        ArgumentNullException.ThrowIfNull(accessMenus, nameof(accessMenus));

        try
        {
            var identityRole = new IdentityRole { 
                Id = role.Id, 
                Name = role.Name, 
                NormalizedName = role.NormalizedName,
                ConcurrencyStamp = role.ConcurrencyStamp
            };

            // Agrega el rol a la base de datos
            await _context.Roles.AddAsync(identityRole).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            // Configura los menús de acceso con el ID del rol recién creado
            foreach (var accessMenu in accessMenus)
            {
                accessMenu.RolId = role.Id;
            }

            // Agrega la lista completa de menús de acceso
            await _context.AccessMenus.AddRangeAsync(accessMenus).ConfigureAwait(false);

            // Guarda los cambios y devuelve el resultado
            return await _context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(AddAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Busca un rol por su ID e incluye los menús de acceso relacionados.
    /// </summary>
    /// <param name="id">El ID del rol que se desea buscar.</param>
    /// <returns>El rol encontrado con sus menús de acceso relacionados o <c>null</c> si no se encuentra.</returns>
    /// <exception cref="ArgumentNullException">Lanzado si el ID es <c>null</c> o vacío.</exception>
    public async Task<AspNetRole?> FindAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id), "El ID no puede ser nulo o vacío.");

        try
        {
            // Buscar los menus del rol
            ICollection<AccessMenu> accessMenuRoles = await _context.AccessMenus
                                                         .Where(y => y.RolId == id)
                                                         .Select(x => new AccessMenu
                                                         {
                                                            MenuId = x.MenuId,
                                                         })
                                                         .ToListAsync();

            var role = await _context.Roles
                            .Where(role => role.Id == id)
                            .Select(role => new AspNetRole
                            {
                                Id = role.Id,
                                Name = role.Name,
                                AccessMenus = accessMenuRoles
                            })
                            .FirstOrDefaultAsync();

            return role;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(FindAsync), ex);
            return null;
        }
    }

    /// <summary>
    /// Modificar un rol y su lista asociada de menús de acceso de manera asincrónica.
    /// </summary>
    /// <param name="role">El rol que se modificar.</param>
    /// <param name="accessMenus">La lista de menús de acceso relacionados con el rol.</param>
    /// <returns>Un valor booleano que indica si la operación fue exitosa.</returns>
    public async Task<bool> UpdateAsync(AspNetRole role, List<AccessMenu> accessMenus)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        ArgumentNullException.ThrowIfNull(accessMenus, nameof(accessMenus));

        try
        {
            var identityRole = new IdentityRole
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.NormalizedName,
                ConcurrencyStamp = role.ConcurrencyStamp
            };

            // Marca el rol como modificado
            _context.Entry(identityRole).State = EntityState.Modified;

            // Elimina todos los permisos actuales del rol
            var existingAccessMenus = _context.AccessMenus.Where(x => x.RolId == identityRole.Id);
            _context.AccessMenus.RemoveRange(existingAccessMenus);

            // Añade los nuevos permisos del rol
            _context.AccessMenus.AddRange(accessMenus);

            // Guarda los cambios en la base de datos
            return await _context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Obtiene todos los registros de roles.
    /// </summary>
    /// <returns>Una colección enumerable de objetos <see cref="AspNetRole"/>.</returns>
    public async Task<IEnumerable<AspNetRole>> GetAllAsync()
    {
        try
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .Select(x => new AspNetRole
                {
                    Id = x.Id,
                    Name = x.Name,
                    NormalizedName = x.NormalizedName,
                    ConcurrencyStamp = x.ConcurrencyStamp
                })
                .ToListAsync();

            return roles;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllAsync), ex);
            return Enumerable.Empty<AspNetRole>();
        }

    }

    /// <summary>
    /// Obtiene una lista de identificadores de roles asociados a un usuario específico.
    /// </summary>
    /// <param name="userId">El identificador del usuario.</param>
    /// <returns>Una lista de strings con los identificadores de los roles asociados al usuario.</returns>
    public async Task<List<string>> GetAllByUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "El identificador del usuario no puede ser nulo o vacío.");


        try
        {
            var roleIds = await (from rol in _context.Roles
                                         join userRol in _context.UserRoles on rol.Id equals userRol.RoleId
                                         join appRol in _context.ApplicationRoles on rol.Id equals appRol.RolId
                                         where userRol.UserId == userId
                                         && appRol.ApplicationId == (int)Domain.Enums.Application.WebAppBase
                                         select rol.Id
                                    ).ToListAsync();

            return roleIds;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllByUserAsync), ex);
            return [];
        }

    }

    /// <summary>
    /// Obtiene la lista de permisos que tiene un usuario.
    /// </summary>
    /// <param name="userId">El identificador del usuario.</param>
    /// <returns>Una lista de números de permiso.</returns>
    public async Task<List<int>> GetAllPermissionsByUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "El identificador del usuario no puede ser nulo o vacío.");


        try
        {
            // Construir la consulta para obtener los permisos
            var permissions = await (from rol in _context.Roles
                                       join userRol in _context.UserRoles on rol.Id equals userRol.RoleId
                                       join accessMenu in _context.AccessMenus on userRol.RoleId equals accessMenu.RolId
                                       join menu in _context.Menus on accessMenu.MenuId equals menu.Id
                                       where userRol.UserId == userId
                                             && menu.PermissionNumber != null
                                             && menu.ApplicationId == (int)Domain.Enums.Application.WebAppBase
                                       select (int)menu.PermissionNumber
                                    ).ToListAsync();

            return permissions;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllPermissionsByUserAsync), ex);
            return new List<int>();
        }

    }


    /// <summary>
    /// Obtiene las opciones del menú a las que tiene acceso el usuario.
    /// </summary>
    /// <param name="userId">El identificador del usuario.</param>
    /// <returns>Una lista de opciones de menú a las que tiene acceso el usuario.</returns>
    /// <exception cref="ArgumentNullException">Lanzado cuando el <paramref name="userId"/> es nulo o vacío.</exception>
    public async Task<List<GetMenuByUserIdDto>> GetMenuOptionsByUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "El identificador del usuario no puede ser nulo o vacío.");


        try
        {            
            var userIdParam = new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = userId };
            var appIdParam = new SqlParameter("@ApplicationId", SqlDbType.Int) { Value = (int)Domain.Enums.Application.WebAppBase };

            // Ejecutar la consulta SQL
            var result = await _context.Set<GetMenuByUserIdDto>()
                                        .FromSqlRaw("EXEC usp_GetMenuByUserId @UserId, @ApplicationId", userIdParam, appIdParam)
                                        .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllPermissionsByUserAsync), ex);
            return [];
        }

    }

    /// <summary>
    /// Obtiene todos los registros de roles.
    /// </summary>
    /// <param name="userId">El identificador del usuario.</param>
    /// <returns>Una colección enumerable de objetos <see cref="AspNetRole"/>.</returns>
    public async Task<IEnumerable<AspNetRole>> GetAllRolesByUserAsync(string userId)
    {
        try
        {
            var roles = await (from userRole in _context.UserRoles
                                join role in _context.Roles on userRole.RoleId equals role.Id
                                where userRole.UserId == userId
                                select new AspNetRole
                                {
                                    Id = role.Id,
                                    Name = role.Name,
                                    NormalizedName = role.NormalizedName,
                                    ConcurrencyStamp = role.ConcurrencyStamp
                                }).ToListAsync();            

            return roles;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllAsync), ex);
            return Enumerable.Empty<AspNetRole>();
        }

    }

}
