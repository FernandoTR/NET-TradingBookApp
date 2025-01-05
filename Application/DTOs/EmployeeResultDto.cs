using Infrastructure;

namespace Application.DTOs;

/// <summary>
/// DTO para regresar el resultado de la confirmación de los correos
/// </summary>
public class EmployeeResultDto
{
    public bool Verification { get; set; }
    public string? MessageError { get; set; }
    public Employee? Employee { get; set; }
}
