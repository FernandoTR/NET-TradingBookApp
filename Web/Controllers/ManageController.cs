using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Application.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using Microsoft.AspNetCore.Identity;
using QRCoder;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Web.Controllers;

[Authorize]
public class ManageController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IUserEmailService _userEmailService;
    private readonly IUserService _userService;
    private readonly IQrCodeGeneratorService _qrCodeGeneratorService;

    public ManageController(IIdentityService identityService, 
                            IEmployeeService employeeService,
                            ILogService logService,
                            IMessageService messageService,
                            IUserEmailService userEmailService,
                            IUserService userService,
                            IQrCodeGeneratorService qrCodeGeneratorService)
    {
        _identityService = identityService;
        _employeeService = employeeService;
        _logService = logService;
        _messageService = messageService;
        _userEmailService = userEmailService;
        _userService = userService;
        _qrCodeGeneratorService = qrCodeGeneratorService;
    }

    // GET: /Manage/Index
    public async Task<ActionResult> Index(ManageMessageId? message)
    {       

        ViewBag.StatusMessage =
            message == ManageMessageId.ChangePasswordSuccess ? "Su contraseña se ha cambiado."
            : message == ManageMessageId.SetPasswordSuccess ? "Su contraseña se ha establecido."
            : message == ManageMessageId.SetTwoFactorSuccess ? "Su proveedor de autenticación de dos factores se ha establecido."
            : message == ManageMessageId.Error ? "Se ha producido un error."
            : message == ManageMessageId.AddPhoneSuccess ? "Se ha agregado su número de teléfono."
            : message == ManageMessageId.RemovePhoneSuccess ? "Se ha quitado su número de teléfono."
            : "";

        var account = await GetUserAccountAsync();
        var userId = account.AspNetUser.Id;
        var model = new IndexViewModel
        {
            HasPassword = account.AspNetUser.PasswordHash != null,
            PhoneNumber = await _identityService.GetPhoneNumberAsync(userId),
            TwoFactor = await _identityService.GetTwoFactorEnabledAsync(userId),
            Logins = await _identityService.GetLoginsAsync(userId),
            BrowserRemembered = await _identityService.IsTwoFactorClientRememberedAsync(userId)
        };        

        ViewBag.FullName = $"{account.Employee.Name} {account.Employee.LastName}";
        ViewBag.Email = account.Employee.Email;
        ViewBag.Location = account.Employee.Location;
        ViewBag.Roles = string.Join(", ", account.AspNetUser.AspNetUserRoles.Select(role => role.Role.Name));

        return View(model);
    }

    /// <summary>
    /// Muestra un resumen general de la cuenta del usuario.
    /// </summary>
    /// <returns>Vista parcial con los datos del resumen.</returns>
    public async Task<PartialViewResult> Overview()
    {
        var account = await GetUserAccountAsync();

        var model = new OverviewViewModel
        {
            EmployeeNumber = account.Employee.Number,
            FullName = $"{account.Employee.Name} {account.Employee.LastName}",
            Email = account.Employee.Email,
            Location = account.Employee.Location,
            Roles = string.Join(", ", account.AspNetUser.AspNetUserRoles.Select(role => role.Role.Name)),
            EmailConfirmed = account.AspNetUser.EmailConfirmed ? "SI" : "NO",
            PhoneNumber = account.AspNetUser.PhoneNumber
        };

        return PartialView("_Overview", model);
    }

    /// <summary>
    /// Muestra la configuración de la cuenta del usuario.
    /// </summary>
    /// <returns>Vista parcial con la configuración.</returns>
    public async Task<PartialViewResult> Settings()
    {
        var account = await GetUserAccountAsync();

        var model = new OverviewViewModel
        {
            Email = account.Employee.Email,
            TwoFactor = await _identityService.GetTwoFactorEnabledAsync(account.AspNetUser.Id)
        };

        return PartialView("_Settings", model);
    }

    /// <summary>
    /// Muestra el registro de eventos de la cuenta.
    /// </summary>
    /// <returns>Vista parcial de los logs.</returns>
    public async Task<PartialViewResult> Logs()
    {
        return PartialView("_Logs");
    }

 

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel model)
    {
        try
        {
            // Validar que los parámetros no estén vacíos
            if (string.IsNullOrWhiteSpace(model.NewEmail) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new ResultBackViewModel 
                { 
                    Success = false, 
                    Message = "El correo electrónico y la contraseña son obligatorios.", 
                    notificationType = NotificationType.Warning 
                });
            }

            // Obtener la cuenta del usuario actual
            var account = await GetUserAccountAsync();

            // Validar las credenciales del usuario
            var isPasswordValid = await _identityService.CheckPasswordSignInAsync(account.AspNetUser.Email, model.Password);
            if (!isPasswordValid.Succeeded)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "Credenciales no válidas.",
                    notificationType = NotificationType.Warning
                });
            }

            // Actualizar el correo electrónico
            var isEmailUpdated = await _employeeService.UpdateEmailAsync(account.Employee.Id, model.NewEmail.Trim());
            if (!isEmailUpdated)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = _messageService.GetResourceError("GenericError"),
                    notificationType = NotificationType.Error
                });
            }

            // Mandar solicitud de confirmacion de correo
            bool mailsent = await _userEmailService.SendEmailConfirmationAsync(account.Employee.Name,
                                                                               model.NewEmail.Trim(),
                                                                               account.Employee.ConfirmationHash,
                                                                               account.Employee.ConfirmationHashEndDate ?? DateTime.Now,
                                                                               UserTypes.User);

            if (!mailsent)
            {
                var ex = new Exception(_messageService.GetResourceError("ErrorSendingMail"));
                _logService.ErrorLog("ManageController.ChangeEmail", ex);
            }

            // Renovar la cookie con las nuevas credenciales sin cerrar sesión 
            var signInResult = await _identityService.RefreshSignInAsync(model.NewEmail);
            if (!signInResult.Succeeded)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "No se pudo iniciar sesión con el nuevo correo electrónico.",
                    notificationType = NotificationType.Error
                });
            }

            // Retornar éxito
            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = "El correo electrónico se actualizó con exito.",
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(ChangeEmail)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
    }

    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        try
        {
            // Validar que los parámetros no estén vacíos
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "La contraseña actual y la nueva contraseña son obligatorias.",
                    notificationType = NotificationType.Warning
                });
            }

            // Obtener la cuenta del usuario actual
            var account = await GetUserAccountAsync();

            // Validar las credenciales del usuario
            var isPasswordValid = await _identityService.CheckPasswordSignInAsync(account.AspNetUser.Email, model.CurrentPassword);
            if (!isPasswordValid.Succeeded)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "Credenciales no válidas.",
                    notificationType = NotificationType.Warning
                });
            }
           

            // Validar que la nueva contraseña no coincida con las últimas 5 contraseñas del usuario
            if (!await _userService.ValidatePasswordHistoryAsync(account.AspNetUser.Id, model.NewPassword, 5))
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = _messageService.GetResourceError("RepeatedPassword"),
                    notificationType = NotificationType.Warning
                });
            }

            var token = await _identityService.GeneratePasswordResetTokenAsync(account.AspNetUser.Id);

            // Intentar restablecer la contraseña del usuario
            var resetResult = await _identityService.ResetPasswordAsync(account.AspNetUser.Id, token, model.NewPassword);
            if (!resetResult.Succeeded)
            {               
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "No se pudo actualizar la contraseña",
                    notificationType = NotificationType.Warning
                });
            }

            // Guardar la nueva contraseña en el historial
            await _userService.SavePasswordHistoryAsync(account.AspNetUser.Id, account.AspNetUser.PasswordHash);

            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = "La contraseña se actualizó con exito",
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(ChangePassword)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
    }

    /// <summary>
    ///  Generar una clave única para vincular la cuenta con el autenticador.
    /// </summary>
    public async Task<IActionResult> EnableAuthenticator()
    {
        string sharedKey = string.Empty;
        string authenticatorUri = string.Empty;

        try
        {
            // Obtener la cuenta del usuario actual
            var account = await GetUserAccountAsync();

            var key = await _identityService.EnableAuthenticator(account.AspNetUser.Id);

            sharedKey = FormatKey(key);
            authenticatorUri = GenerateQrCodeUri(account.AspNetUser.Email,key);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(EnableAuthenticator)}", ex);
        }       

        return View(new EnableAuthenticatorViewModel
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri
        });
    }

    /// <summary>
    ///  Generar la imagen del código QR en formato svg
    /// </summary>
    [HttpGet("Generate")]
    public IActionResult Generate(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest("El contenido no puede estar vacío.");
        }

        try
        {
            var qrCodeSvg = _qrCodeGeneratorService.GenerateQrCodeSvg(content);

            // Devuelve el código QR como archivo SVG
            return File(
                System.Text.Encoding.UTF8.GetBytes(qrCodeSvg),
                "image/svg+xml",
                "QRCode.svg"
            );
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(Generate)}", ex);
            return StatusCode(500, new { message = "Ocurrió un error generando el código QR." });
        }
    }


    /// <summary>
    /// Verificar el Código del Autenticador
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyAuthenticatorCode([FromBody] string code)
    {    
        try
        {
            // Validar que los parámetros no estén vacíos
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "El código es obligatorio.",
                    notificationType = NotificationType.Warning
                });
            }

            // Obtener la cuenta del usuario actual
            var account = await GetUserAccountAsync();

            var isValid = await _identityService.VerifyTwoFactorTokenAsync(account.AspNetUser.Id, code);

            if (!isValid)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "El código no es válido.",
                    notificationType = NotificationType.Warning
                });
            }

            await _identityService.SetTwoFactorEnabledAsync(account.AspNetUser.Id);

            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = "La autenticación de dos factores se habilitó correctamente.",
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(EnableAuthenticator)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
    }





    #region Aplicaciones auxiliares

    /// <summary>
    /// Obtiene la información completa de la cuenta del usuario actual.
    /// </summary>
    /// <returns>Objeto de cuenta o null si no se encuentra.</returns>
    private async Task<EmployeeDto?> GetUserAccountAsync()
    {
        var currentUser = _identityService.GetCurrentUserAsync();
        if (currentUser == null) return null;

        var accounts = await _employeeService.GetAllAsync();
        return accounts.FirstOrDefault(x => x.AspNetUser.Id == currentUser.UserId);
    }


    /// <summary>
    /// Da formato a la clave (authenticator key) que se genera para el usuario.
    /// </summary>
    private string FormatKey(string unformattedKey)
    {
        if (string.IsNullOrEmpty(unformattedKey))
        {
            return string.Empty;
        }

        // Divide la clave en bloques de 4 caracteres separados por un guion
        var formattedKey = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            formattedKey.Append(unformattedKey.Substring(currentPosition, 4)).Append("-");
            currentPosition += 4;
        }

        // Añade los caracteres restantes (si los hay)
        formattedKey.Append(unformattedKey.Substring(currentPosition));

        return formattedKey.ToString().ToUpperInvariant(); // Opcional: convierte a mayúsculas
    }
    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            "otpauth://totp/{0}?secret={1}&issuer={2}&digits=6",
            Uri.EscapeDataString("WebAppBase"),
            unformattedKey,
            Uri.EscapeDataString(email)
        );
    }

    #endregion

}
