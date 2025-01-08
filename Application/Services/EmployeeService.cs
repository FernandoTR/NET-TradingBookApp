
using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IRolesRepository _rolesRepository;

    public EmployeeService(IEmployeeRepository repository, IUserRepository userRepository, IRolesRepository rolesRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
        _rolesRepository = rolesRepository;
    }


    public async Task<bool> AddEmployeeAsync(Employee employee)
    {
        return await _repository.AddAsync(employee);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _repository.FindAsync(id);
    }

    public async Task<EmployeeResultDto> ConfirmEmployeeEmailAsync(string email, string hashVerification)
    {
        return await _repository.ConfirmEmailAsync(email, hashVerification);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
        return await _repository.UpdateAsync(employee);
    }

    public async Task<bool> ToggleEmployeeStatusAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> CheckEmailExistsAsync(string email, string emailTemp)
    {
        return await _repository.ExistsByEmailAsync(email, emailTemp);
    }

    public async Task<bool> CheckNumberExistsAsync(string employeeNumber, string employeeNumberTemp)
    {
        return await _repository.ExistsByNumberAsync(employeeNumber, employeeNumberTemp);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        // Obtiene la lista de empleados desde el repositorio
        var employees = await _repository.GetAllAsync();

        // Ejecutar las tareas asincrónicas para recuperar los datos de los usuarios.
        var aspNetUserTasks = employees.Select(e => _userRepository.FindByEmployeeAsync(e.Id).Result).ToList();

        // Mapear los resultados a EmployeeDto.
        var employeeDtos = employees.Select((employee, index) => new EmployeeDto
        {
            Employee = employee,
            AspNetUser = aspNetUserTasks.Where(x => x.EmployeeId == employee.Id).FirstOrDefault(),
        }).ToList();        

        return employeeDtos;
    }

    public async Task<bool> UpdateEmailAsync(int employeeId, string newEmail)
    {
        return await _repository.UpdateEmailAsync(employeeId, newEmail);
    }


}
