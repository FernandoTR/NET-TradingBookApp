using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class ExternalLoginConfirmationViewModel
{
    [Required]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; }
}

public class ExternalLoginListViewModel
{
    public string ReturnUrl { get; set; }
}

public class SendCodeViewModel
{
    public string ProviderSelected { get; set; } = string.Empty;
    public IList<string>? Providers { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public required string UserId { get; set; }
}

public class VerifyCodeViewModel
{
    [Required]
    public required string Provider { get; set; }

    [Required]
    [Display(Name = "Código")]
    public required string Code { get; set; }
    public string? ReturnUrl { get; set; }

    [Display(Name = "¿Recordar este explorador?")]
    public bool RememberBrowser { get; set; }

    public bool RememberMe { get; set; }
    public required string UserId { get; set; } 
}

public class ForgotViewModel
{
    [Required]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; }
}

public class LoginViewModel
{
    [Required]
    [Display(Name = "Correo electrónico")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public required string Password { get; set; }

    [Display(Name = "¿Recordar cuenta?")]
    public bool RememberMe { get; set; } = false;
}

public class RegisterViewModel
{
    [HiddenInput]
    public int EmployeeId { get; set; }

    [Required]
    public List<RolUser> Roles { get; set; }


    [EmailAddress]
    [Display(Name = "Usuario")]
    public string Email { get; set; }
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {2}.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
    public string ConfirmPassword { get; set; }

    public string Code { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; }
}
