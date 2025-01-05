using System.ComponentModel;

namespace Domain.Enums;

/// <summary>
/// Representa los roles posibles de un usuario.
/// </summary>
public enum UserRoles
{
    [Description("Desconocido")]
    Unknow = 0,
    [Description("Administrador")]
    Admin = 1

}
