

using Application.DTOs;
using Infrastructure;

namespace Application.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> AddAsync(Employee employee);
    Task<EmployeeResultDto> ConfirmEmailAsync(string email, string hashVerification);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByEmailAsync(string email, string emailTemp);
    Task<bool> ExistsByNumberAsync(string employeeNumber, string employeeNumberTemp);
    Task<Employee?> FindAsync(int id);
    Task<bool> UpdateAsync(Employee employee);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<bool> UpdateEmailAsync(int employeeId, string newEmail);
}
