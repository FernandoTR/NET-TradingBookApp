using System.ComponentModel;

namespace Web.Models;

public class EmployeesViewModel
{
    public int EmployeeId { get; set; }
    public DateTime CreationDate { get; set; }
    public string AuthorId { get; set; }
    public DateTime? AlterDate { get; set; }
    public string? AlterAuthorId { get; set; }

    [DisplayName("Número de Empleado")]
    public string Number { get; set; }
    [DisplayName("Nombres")]
    public string Name { get; set; }
    [DisplayName("Apellidos")]
    public string LastName { get; set; }
    [DisplayName("Correo")]
    public string Email { get; set; }
    [DisplayName("Ubicación")]
    public string Location { get; set; }
    public bool ConfirmedEmail { get; set; }
    public string? ConfirmationHash { get; set; }
    public DateTime? ConfirmationHashEndDate { get; set; }
    public int StatusEmployeeId { get; set; }
    [DisplayName("Estado")]
    public string? StatusName { get; set; }
    public bool HaveUser { get; set; }
    public string? Task { get; set; }

    /// campos para las validaciones 
    public string? EmployeeNumberTemp { get; set; }
    public string? EmailTemp { get; set; }
}
