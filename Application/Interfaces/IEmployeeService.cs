using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface IEmployeeService
{
    Task<bool> AddEmployeeAsync(Employee employee);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<EmployeeResultDto> ConfirmEmployeeEmailAsync(string email, string hashVerification);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateEmployeeAsync(Employee employee);
    Task<bool> ToggleEmployeeStatusAsync(int id);
    Task<bool> CheckEmailExistsAsync(string email, string emailTemp);
    Task<bool> CheckNumberExistsAsync(string employeeNumber, string employeeNumberTemp);
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<bool> UpdateEmailAsync(int employeeId, string newEmail);
}
