using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Web.Models;

public class UsersListViewModel
{
    public string Id { get; set; }
    [DisplayName("ID")]
    public int EmployeeId { get; set; }
    [DisplayName("Id Empleado")]
    public string? EmployeeNumber { get; set; }
    [DisplayName("Numero Empleado")]
    public string? Email { get; set; }
    [DisplayName("Email")]
    public string? UserName { get; set; }
    [DisplayName("Usuario")]
    public string? PhoneNumber { get; set; }
    [DisplayName("Numero Telefonico")]
    public bool? PhoneNumberConfirmed { get; set; }
    [DisplayName("Telefono Confirmado")]
    public int? AccesFailedCount { get; set; }
    [DisplayName("Accesos Fallidos")]

    public bool? Enable { get; set; }

    public bool? EmailConfirmed { get; set; }
    [DisplayName("Email Confirmado")]
    public int? StatusEmployeeId { get; set; }
    [DisplayName("Estatus Empleado")]
    public DateTime? PasswordEndDate { get; set; }
    [Display(Name = "Expiracion Contraseña")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]


    [DisplayName("Fecha Formateada")]
    public string? FormatedDate { get; set; }
    public int? UserRolesId { get; set; }
    [DisplayName("Rol Id")]
    public string? Name { get; set; }
    [DisplayName("Rol")]
    public bool? ResetFlag { get; set; }
    [DisplayName("Bandera Reseteo")]
    public string? Task { get; set; }
    public string? Status { get; set; }


    public List<UsersRolesViewModel> listAccess { get; set; }
}

public class UsersRolesViewModel
{
    public string RolId { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
}