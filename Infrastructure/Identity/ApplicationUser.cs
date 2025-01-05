using Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Infrastructure.Identity; 

public class ApplicationUser : IdentityUser
{
   

    /// <summary>
    /// Identificador del empleado asociado al usuario.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Fecha de expiración de la contraseña.
    /// </summary>
    public DateTime PasswordEndDate { get; set; }

    /// <summary>
    /// Indica si el usuario está habilitado.
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// Bandera para indicar si se requiere un reinicio de sesión u otro proceso relacionado.
    /// </summary>
    public bool ResetFlag { get; set; }

    /// <summary>
    /// Identificador del tipo de usuario (rol personalizado o categorización específica).
    /// </summary>
    public int UserTypeId { get; set; }


    


}
