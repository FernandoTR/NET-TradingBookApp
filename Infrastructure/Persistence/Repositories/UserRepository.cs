using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ILogService _logService;
    private readonly IRolesRepository _roleRepository;
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context, ILogService logService, IRolesRepository roleRepository)
    {
        _context = context;
        _logService = logService;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// Obtiene la información del usuario actual, combinando los datos de ApplicationUser y Employee,
    /// así como roles, permisos y opciones de menú asociadas.
    /// </summary>
    /// <param name="user">Objeto ApplicationUser que representa al usuario actual.</param>
    /// <returns>Un objeto CurrentUserDto con la información del usuario o null en caso de error.</returns>
    public async Task<CurrentUserDto?> GetCurrentUserInformation(CurrentUserDto user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "El usuario no puede ser nulo.");

        try
        {
            // Inicializamos el objeto CurrentUserDto con los datos básicos de ApplicationUser.
            var currentUser = new CurrentUserDto()
            {
                UserId = user.UserId,
                Email = user.Email,
                Enable = user.Enable,
                EmployeeId = user.EmployeeId,
                ResetFlag = user.ResetFlag
            };

            //Completar información con los datos de empleado.
            var employee = await _context.Employees
                 .AsNoTracking()
                 .FirstOrDefaultAsync(x => x.Id == user.EmployeeId);

            if (employee != null)
            {
                currentUser.Name = employee.Name;
                currentUser.LastName = employee.LastName;
                currentUser.EmailConfirmed = employee.ConfirmedEmail;
                currentUser.Number = employee.Number;
            }

            // Obtenemos roles, permisos y opciones de menú asociadas al usuario.
            currentUser.RolIdList = await _roleRepository.GetAllByUserAsync(user.UserId);
            currentUser.PermissionNumberList = await _roleRepository.GetAllPermissionsByUserAsync(user.UserId);
            currentUser.MenuOptions = await _roleRepository.GetMenuOptionsByUserAsync(user.UserId);

            // Validamos el estado general del usuario.
            currentUser.IsValid = currentUser.EmailConfirmed && currentUser.Enable && !currentUser.ResetFlag;

            return currentUser;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetCurrentUserInformation), ex);
            return null;
        }
    }

    /// <summary>
    /// Busca un usuario de tipo AspNetUser asociado a un empleado por su ID.
    /// </summary>
    /// <param name="id">ID del empleado que se desea buscar.</param>
    /// <returns>Un objeto AspNetUser si se encuentra, o null si no se encuentra o si ocurre un error.</returns>
    public async Task<AspNetUser> FindByEmployeeAsync(int id)
    {       
        try
        {
            // Consulta en la base de datos para obtener el usuario por EmployeeId.
            var users = await _context.Users
                .AsNoTracking()
                .Where(x => x.EmployeeId == id)
                .Select(x => new AspNetUser
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    PasswordEndDate = x.PasswordEndDate,
                    Enable = x.Enable,
                    ResetFlag = x.ResetFlag,
                    UserTypeId = x.UserTypeId,
                    Email = x.Email,
                    EmailConfirmed = x.EmailConfirmed,
                    PasswordHash = x.PasswordHash,
                    SecurityStamp = x.SecurityStamp,
                    PhoneNumber = x.PhoneNumber,
                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                    TwoFactorEnabled = x.PhoneNumberConfirmed,
                    LockoutEnabled = x.LockoutEnabled,
                    AccessFailedCount = x.AccessFailedCount,
                    UserName = x.UserName,
                    ConcurrencyStamp = x.ConcurrencyStamp,
                    NormalizedUserName = x.NormalizedUserName,
                    NormalizedEmail = x.NormalizedEmail                    
                })
                .FirstOrDefaultAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(FindByEmployeeAsync), ex);
            return null;
        }
    }

    /// <summary>
    /// Busca un usuario de tipo AspNetUser por su correo.
    /// </summary>
    /// <param name="email">Correo del usuario que se desea buscar.</param>
    /// <returns>Un objeto AspNetUser si se encuentra, o null si no se encuentra o si ocurre un error.</returns>
    public async Task<AspNetUser> FindByEmailAsync(string email)
    {
        try
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(x => x.Email == email)
                .Select(x => new AspNetUser
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    PasswordEndDate = x.PasswordEndDate,
                    Enable = x.Enable,
                    ResetFlag = x.ResetFlag,
                    UserTypeId = x.UserTypeId,
                    Email = x.Email,
                    EmailConfirmed = x.EmailConfirmed,
                    PasswordHash = x.PasswordHash,
                    SecurityStamp = x.SecurityStamp,
                    PhoneNumber = x.PhoneNumber,
                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                    TwoFactorEnabled = x.PhoneNumberConfirmed,
                    LockoutEnabled = x.LockoutEnabled,
                    AccessFailedCount = x.AccessFailedCount,
                    UserName = x.UserName,
                    ConcurrencyStamp = x.ConcurrencyStamp,
                    NormalizedUserName = x.NormalizedUserName,
                    NormalizedEmail = x.NormalizedEmail
                })
                .FirstOrDefaultAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(FindByEmailAsync), ex);
            return null;
        }
    }

    /// <summary>
    /// Busca un usuario de tipo AspNetUser por su ID.
    /// </summary>
    /// <param name="id">Llave primaria del Usuario.</param>
    /// <returns>Un objeto AspNetUser si se encuentra, o null si no se encuentra o si ocurre un error.</returns>
    public async Task<AspNetUser> FindAsync(int id)
    {
        try
        {
            // Buscar al usuario en la base de datos usando su identificador.
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == Convert.ToString(id))
                .Select(x => new AspNetUser
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    PasswordEndDate = x.PasswordEndDate,
                    Enable = x.Enable,
                    ResetFlag = x.ResetFlag,
                    UserTypeId = x.UserTypeId,
                    Email = x.Email,
                    EmailConfirmed = x.EmailConfirmed,
                    PasswordHash = x.PasswordHash,
                    SecurityStamp = x.SecurityStamp,
                    PhoneNumber = x.PhoneNumber,
                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                    TwoFactorEnabled = x.PhoneNumberConfirmed,
                    LockoutEnabled = x.LockoutEnabled,
                    AccessFailedCount = x.AccessFailedCount,
                    UserName = x.UserName,
                    ConcurrencyStamp = x.ConcurrencyStamp,
                    NormalizedUserName = x.NormalizedUserName,
                    NormalizedEmail = x.NormalizedEmail
                })
                .FirstOrDefaultAsync();
            

            // Si el usuario no se encuentra, retornar null.
            return user;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(FindAsync), ex);
            return null;
        }
    }

    /// <summary>
    /// Busca un usuario de tipo AspNetUser por su ID.
    /// </summary>
    /// <param name="id">Llave primaria del Usuario.</param>
    /// <returns>Un objeto AspNetUser si se encuentra, o null si no se encuentra o si ocurre un error.</returns>
    public async Task<AspNetUser> FindAsync(string id)
    {
        try
        {
            // Buscar al usuario en la base de datos usando su identificador.
            var user = await _context.Users
               .AsNoTracking()
               .Where(x => x.Id == id)
               .Select(x => new AspNetUser
               {
                   Id = x.Id,
                   EmployeeId = x.EmployeeId,
                   PasswordEndDate = x.PasswordEndDate,
                   Enable = x.Enable,
                   ResetFlag = x.ResetFlag,
                   UserTypeId = x.UserTypeId,
                   Email = x.Email,
                   EmailConfirmed = x.EmailConfirmed,
                   PasswordHash = x.PasswordHash,
                   SecurityStamp = x.SecurityStamp,
                   PhoneNumber = x.PhoneNumber,
                   PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                   TwoFactorEnabled = x.PhoneNumberConfirmed,
                   LockoutEnabled = x.LockoutEnabled,
                   AccessFailedCount = x.AccessFailedCount,
                   UserName = x.UserName,
                   ConcurrencyStamp = x.ConcurrencyStamp,
                   NormalizedUserName = x.NormalizedUserName,
                   NormalizedEmail = x.NormalizedEmail
               })
               .FirstOrDefaultAsync();

            // Si el usuario no se encuentra, retornar null.
            return user;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(FindAsync), ex);
            return null;
        }
    }

    /// <summary>
    /// Actualiza la información de un usuario en la base de datos de forma asíncrona.
    /// </summary>
    /// <param name="user">Instancia de <see cref="AspNetUser"/> que representa el usuario a actualizar.</param>
    /// <returns>Un valor booleano que indica si la operación fue exitosa.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el parámetro <paramref name="user"/> es nulo.</exception>
    public async Task<bool> UpdateAsync(AspNetUser user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "El usuario no puede ser nulo.");

        try
        {
            // Marca la entidad como modificada
            _context.Entry(user).State = EntityState.Modified;

            // Guarda los cambios en la base de datos y verifica el resultado
            return await _context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Verifica si el hash de la contraseña coincide con una contraseña proporcionada.
    /// Utiliza el PasswordHasher<TUser> de Microsoft.AspNetCore.Identity.
    /// </summary>
    /// <param name="passwordHash">El hash de la contraseña previamente almacenado.</param>
    /// <param name="password">La contraseña en texto plano que se desea verificar.</param>
    /// <returns>
    /// Un valor booleano que indica si la contraseña proporcionada coincide con el hash (true si coincide, false en caso contrario).
    /// </returns>
    public bool VerifyPasswordHash(string passwordHash, string password)
    {

        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentException("El hash de la contraseña no puede ser nulo o vacío.", nameof(passwordHash));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("La contraseña no puede ser nula o vacía.", nameof(password));

        try
        {
            // Utilizamos el PasswordHasher genérico para verificar la contraseña.
            var passwordHasher = new PasswordHasher<object>();

            // Se realiza la verificación del hash contra la contraseña proporcionada.
            var verificationResult = passwordHasher.VerifyHashedPassword(default, passwordHash, password);

            // Retorna true si el resultado de la verificación es exitoso.
            return verificationResult == PasswordVerificationResult.Success;
        }
        catch (Exception ex)
        {
            // Registra el error en el servicio de logging
            _logService.ErrorLog(nameof(VerifyPasswordHash), ex);
            return false;
        }
    }

    /// <summary>
    /// Valida que una nueva contraseña no coincida con las contraseñas recientes del usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="newPassword">Nueva contraseña a verificar.</param>
    /// <param name="historyQuantity">Cantidad de contraseñas históricas a considerar.</param>
    /// <returns>Devuelve true si la nueva contraseña es válida; de lo contrario, false.</returns>
    public async Task<bool> ValidatePasswordHistoryAsync(string userId, string newPassword, int historyQuantity)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("El ID de usuario no puede ser nulo o vacío.", nameof(userId));

        if (string.IsNullOrEmpty(newPassword))
            throw new ArgumentException("La nueva contraseña no puede ser nula o vacía.", nameof(newPassword));

        if (historyQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(historyQuantity), "La cantidad de historial debe ser mayor que cero.");

        try
        {
            // Obtener los últimos hashes de contraseñas del usuario
            var lastPasswords = await _context.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.UpdateDate)
                .Take(historyQuantity)
                .Select(ph => ph.PasswordHash)
                .ToListAsync();

            // Si no hay contraseñas en el historial, la nueva contraseña es válida
            if (!lastPasswords.Any())
                return true;

            // Verificar si la nueva contraseña coincide con alguna en el historial
            return !lastPasswords.Any(hash => VerifyPasswordHash(hash, newPassword));
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Guarda el historial de contraseñas de un usuario en la base de datos.
    /// </summary>
    /// <param name="userId">ID del usuario para el cual se guarda el historial de contraseñas.</param>
    /// <param name="passwordHash">El hash de la contraseña que se guarda en el historial.</param>
    /// <returns>Devuelve un valor booleano que indica si la operación se realizó con éxito.</returns>
    public async Task<bool> SavePasswordHistoryAsync(string userId, string passwordHash)
    {        
        try
        {
            // Crear una nueva instancia de PasswordHistory con los datos proporcionados
            var passwordHistory = new PasswordHistory
            {
                UserId = userId,
                PasswordHash = passwordHash,
                UpdateDate = DateTime.Now 
            };

            // Agregar la nueva entrada de historial de contraseñas en la base de datos
            await _context.PasswordHistories.AddAsync(passwordHistory);

            // Guardar los cambios en la base de datos y devolver si se guardó correctamente
            return await _context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SavePasswordHistoryAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Método para deshabilitar o habilitar un usuario de la base de datos de manera lógica (cambiando el estado 'Enable').
    /// Si el usuario no existe, lanza una excepción.
    /// </summary>
    /// <param name="id">El ID del usuario que se va a deshabilitar o habilitar.</param>
    /// <returns>Devuelve true si los cambios fueron guardados correctamente, de lo contrario false.</returns>
    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id), "El id de usuario no puede ser nulo.");

        try
        {
            // Buscamos al usuario en la base de datos utilizando el ID proporcionado
            var user = await _context.Users.FindAsync(id);

            // Verificamos si el usuario existe en la base de datos
            if (user == null)
            {
                throw new KeyNotFoundException($"El usuario con ID {id} no fue encontrado.");
            }

            // Cambiamos el valor de 'Enable' (si está habilitado, lo deshabilitamos, y viceversa)
            user.Enable = !user.Enable;

            // Marca la entidad como modificada para que EF Core sepa que se debe actualizar
            _context.Entry(user).State = EntityState.Modified;

            // Guarda los cambios en la base de datos y verifica el resultado
            return await _context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(DeleteAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Verifica si el usuario es de tipo "usuario".
    /// </summary>
    /// <param name="userId">El identificador único del usuario.</param>
    /// <returns>Devuelve un valor booleano que indica si el usuario es válido.</returns>
    public async Task<bool> IsUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId), "El id de usuario no puede ser nulo.");

        try
        {
            // Buscar el usuario en la base de datos
            var fullUserInformation = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId); 

            // Verificar si se encontró el usuario y si su tipo es 'Usuario'
            if (fullUserInformation != null && fullUserInformation.UserTypeId == (int)Domain.Enums.UserTypes.User)
            {
                return true;
            }

            // Si no se encontró un usuario válido, devolver false
            return false;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(IsUserAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Metodo para traer a los usuarios que tengan un permiso en particular
    /// </summary>
    /// <param name="permision"></param>
    /// <returns></returns>
    public async Task<List<AspNetUser>> UsersWithPermissionAsync(Permissions permision)
    {
        try
        {
            var users = await (from user in _context.Users
                                join userRol in _context.UserRoles on user.Id equals userRol.UserId
                                join rol in _context.Roles on userRol.RoleId equals rol.Id
                                join accessMenu in _context.AccessMenus on rol.Id equals accessMenu.RolId
                                join menu in _context.Menus on accessMenu.MenuId equals menu.Id
                                where menu.PermissionNumber == (int)permision
                                select user
                              )
                              //.Include("Employees")
                              .Select(x => new AspNetUser
                               {
                                    Id = x.Id,
                                    EmployeeId = x.EmployeeId,
                                    PasswordEndDate = x.PasswordEndDate,
                                    Enable = x.Enable,
                                    ResetFlag = x.ResetFlag,
                                    UserTypeId = x.UserTypeId,
                                    Email = x.Email,
                                    EmailConfirmed = x.EmailConfirmed,
                                    PasswordHash = x.PasswordHash,
                                    SecurityStamp = x.SecurityStamp,
                                    PhoneNumber = x.PhoneNumber,
                                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                                    TwoFactorEnabled = x.PhoneNumberConfirmed,
                                    LockoutEnabled = x.LockoutEnabled,
                                    AccessFailedCount = x.AccessFailedCount,
                                    UserName = x.UserName,
                                    ConcurrencyStamp = x.ConcurrencyStamp,
                                    NormalizedUserName = x.NormalizedUserName,
                                    NormalizedEmail = x.NormalizedEmail
                               })
                              .ToListAsync(); 

            return users;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UsersWithPermissionAsync), ex);
            return [];
        }

    }

    /// <summary>
    /// Obtiene la información del usuario con sus roles.
    /// </summary>
    /// <param name="userId">UserId</param>
    /// <returns></returns>
    public async Task<AspNetUser> FindUserRolesAsync(string userId)
    {
        try
        {
            // Buscar los roles del usuario en la base de datos usando su identificador.
            ICollection<AspNetUserRole> userRoles = await _context.UserRoles
                                                     .Where(y => y.UserId == userId)
                                                     .Select(x => new AspNetUserRole
                                                     {
                                                         UserId = x.UserId,
                                                         RoleId = x.RoleId
                                                     })
                                                     .ToListAsync();

            var user = await _context.Users
                .Select(x => new AspNetUser
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    PasswordEndDate = x.PasswordEndDate,
                    Enable = x.Enable,
                    ResetFlag = x.ResetFlag,
                    UserTypeId = x.UserTypeId,
                    Email = x.Email,
                    EmailConfirmed = x.EmailConfirmed,
                    PasswordHash = x.PasswordHash,
                    SecurityStamp = x.SecurityStamp,
                    PhoneNumber = x.PhoneNumber,
                    PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                    TwoFactorEnabled = x.PhoneNumberConfirmed,
                    LockoutEnabled = x.LockoutEnabled,
                    AccessFailedCount = x.AccessFailedCount,
                    UserName = x.UserName,
                    ConcurrencyStamp = x.ConcurrencyStamp,
                    NormalizedUserName = x.NormalizedUserName,
                    NormalizedEmail = x.NormalizedEmail,
                    AspNetUserRoles = userRoles,
                })
                .FirstOrDefaultAsync();




            // Si el usuario no se encuentra, retornar null.
            return user;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(FindUserRolesAsync), ex);
            return null;
        }
    }

    /// <summary>
    /// Actualiza la información de un usuario y asigna nuevos roles basados en los IDs proporcionados.
    /// </summary>
    /// <param name="user">El objeto del usuario que se desea actualizar.</param>
    /// <param name="listPermissions">Lista de permisos asignados al rol.</param>
    /// <returns>
    /// Retorna <c>true</c> si los cambios se guardaron exitosamente; de lo contrario, <c>false</c>.
    /// </returns>
    public async Task<bool> UpdateAsync(AspNetUser user, List<AspNetUserRole> listPermissions)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Elimina los permisos actuales del rol
            var existingRoles = _context.UserRoles.Where(x => x.UserId == user.Id);
            _context.UserRoles.RemoveRange(existingRoles);

            // Agrega los nuevos permisos
            List<IdentityUserRole<string>> listPermissionsMenu = listPermissions
            .Select(user => new IdentityUserRole<string>
            {
                UserId = user.UserId,
                RoleId = user.RoleId  
            })
            .ToList();

            if (listPermissionsMenu?.Any() == true)
            {
                await _context.UserRoles.AddRangeAsync(listPermissionsMenu);
            }

            // Guarda los cambios y confirma la transacción si es exitoso
            var changesSaved = await _context.SaveChangesAsync() > 0;

            if (changesSaved)
            {
                await transaction.CommitAsync();
            }

            return changesSaved;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // Revierte los cambios si ocurre un error
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }


}
