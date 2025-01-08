using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    /// <summary>
    /// Metodo para agregar un nuevo empleado
    /// </summary>
    /// <param name="employee"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(Employee employee)
    {
        try
        {
            await _context.Employees.AddAsync(employee);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(AddAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Metodo para buscar un empleado y confirmar email
    /// </summary>
    /// <param name="email"></param>
    /// <param name="hashVerification"></param>
    /// <returns></returns>
    public async Task<EmployeeResultDto> ConfirmEmailAsync(string email, string hashVerification)
    {
        var result = new EmployeeResultDto();

        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email && e.ConfirmationHash == hashVerification);

            if (employee == null)
            {
                result.Verification = false;
                result.MessageError = "No existe el usuario en el sistema.";
                return result;
            }

            if (employee.ConfirmedEmail)
            {
                result.Verification = false;
                result.MessageError = "El correo ya está confirmado.";
                return result;
            }

            if (employee.ConfirmationHashEndDate < DateTime.UtcNow)
            {
                result.Verification = false;
                result.MessageError = "La vigencia del código ha expirado.";
                return result;
            }

            // Actualiza datos empleado
            _context.Entry(employee).State = EntityState.Modified;
            employee.ConfirmedEmail = true;
            _context.Update(employee);


            // Actualiza datos usuario
            var userActive = _context.Users.FirstOrDefault(x => x.EmployeeId == employee.Id);

            if (userActive != null)
            {
                userActive.EmailConfirmed = true;
                _context.Entry(userActive).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            result.Verification = true;
            result.MessageError = "Ahora tu acceso se encuentra activo, en breve el administrador enviará a tu correo electrónico las credenciales de acceso que deberás cambiar la primera vez que inicies sesión.";
            result.Employee = employee;

        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(ConfirmEmailAsync), ex);
        }

        return result;
    }

    /// <summary>
    /// Metodo para habilitar/inhabilitar un empleado
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);

            // Verifica si el empleado existe
            if (employee == null) return false;        

            // Alterna el estado del empleado
            employee.StatusEmployeeId = employee.StatusEmployeeId == (int)EmployeesStatus.Active
                ? (int)EmployeesStatus.Inactive
                : (int)EmployeesStatus.Active;

            // Marca la entidad como modificada
            _context.Entry(employee).State = EntityState.Modified;

            // Guarda los cambios en la base de datos
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(DeleteAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Metodo para buscar un empleado por el email
    /// </summary>
    /// <param name="email">Email del empleado</param>
    /// <param name="emailTemp">Email original en caso que sea modificar</param>
    /// <returns></returns>
    public async Task<bool> ExistsByEmailAsync(string email, string emailTemp)
    {
        try
        {
            return await _context.Employees.AnyAsync(e => e.Email == email && e.Email != emailTemp);

        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(ExistsByEmailAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Busca el empleado por medio del numero de empleado
    /// </summary>
    /// <param name="employeeNumber">numero de empleado</param>
    /// <param name="employeeNumberTemp">numero de empleado original que tiene el empleado</param>
    /// <returns></returns>
    public async Task<bool> ExistsByNumberAsync(string employeeNumber, string employeeNumberTemp)
    {
        try
        {
            //return await _context.Employees.AnyAsync(e => e.Number == employeeNumber && e.Number != employeeNumberTemp);
            //var employee = await (from cc in _context.Employees
            //                where cc.Number.Equals(employeeNumber) && !cc.Number.Equals(employeeNumberTemp)
            //                select cc).FirstOrDefaultAsync();
             var exist = await _context.Employees.AnyAsync(e => e.Number == employeeNumber && e.Number != employeeNumberTemp);

            return exist;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(ExistsByNumberAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Metodo para buscar un empleado
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Employee?> FindAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }


    /// <summary>
    /// Metodo para actualizar la informacion de un empleado
    /// </summary>
    /// <param name="employee"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(Employee employee)
    {
        try
        {
            _context.Update(employee);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Recupera de forma asincrónica todos los empleados de la base de datos.
    /// </summary>
    /// <returns>
    /// Una colección de empleados. Si ocurre un error, retorna una colección vacía.
    /// </returns>
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        try
        {
            var employees = await (from employee in _context.Employees.Include(x => x.StatusEmployee)
                                   join user in _context.Users on employee.Id equals user.EmployeeId into userGroup
                                   from user in userGroup.DefaultIfEmpty()
                                   select employee
                                   ).ToListAsync();

            return employees;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllAsync), ex);
            return Enumerable.Empty<Employee>();
        }

    }

    /// <summary>
    /// Método para cambiar el correo electrónico de un empleado.
    /// Actualiza tanto la información del empleado como la del usuario relacionado en la base de datos.
    /// </summary>
    /// <param name="newEmail">El nuevo correo electrónico que se asignará.</param>
    /// <returns>
    /// Un valor booleano indicando si la operación fue exitosa.
    /// `true` si los cambios se guardaron correctamente; de lo contrario, `false`.
    /// </returns>
    public async Task<bool> UpdateEmailAsync(int employeeId,string newEmail)
    {
        try
        {
            // Iniciar una transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Obtener el empleado desde la base de datos
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null)
            {
                _logService.ErrorLog(nameof(UpdateEmailAsync), new KeyNotFoundException("Empleado no encontrado."));
                return false;
            }

            // Actualizar el correo del empleado
            employee.Email = newEmail;            
            _context.Employees.Update(employee);


            // Obtener el usuario relacionado
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (user == null)
            {
                _logService.ErrorLog(nameof(UpdateEmailAsync), new KeyNotFoundException("Usuario relacionado no encontrado."));
                return false;
            }

            // Actualizar el correo del usuario
            user.Email = newEmail;
            user.EmailConfirmed = false; // desconfirmamos el email 
            user.UserName = newEmail;
            user.NormalizedUserName = newEmail.ToUpper();
            user.NormalizedEmail = newEmail.ToUpper();            
            _context.Users.Update(user);

            // Guardar los cambios
            var changesSaved = await _context.SaveChangesAsync() > 0;

            if (changesSaved)
            {
                // Confirmar la transacción
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }

            return changesSaved;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(UpdateAsync), ex);
            return false;
        }
    }



}
