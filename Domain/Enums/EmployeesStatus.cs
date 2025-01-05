using System.ComponentModel;

namespace Domain.Enums;

/// <summary>
/// Representa los estatus posibles de un empleado.
/// </summary>
public enum EmployeesStatus
{
    [Description("Desconocido")]
    Unknow = 0,
    [Description("Activo")]
    Active = 1,
    [Description("Inactivo")]
    Inactive = 2
}