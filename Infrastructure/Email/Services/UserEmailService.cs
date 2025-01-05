using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Email.Services;

public class UserEmailService : IUserEmailService
{
    private readonly IEmailSender _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;
    private readonly IWebHostEnvironment _env;

    public UserEmailService(IEmailSender emailService,
                            IConfiguration configuration,
                            ILogService logService,
                            IWebHostEnvironment webHostEnvironment)
    {
        _emailService = emailService;
        _configuration = configuration;
        _logService = logService;
        _env = webHostEnvironment;
    }


    /// <summary>
    /// Metodo para enviar el mail unicamente para poder verficar el correo
    /// </summary>
    /// <param name="name">Nombre del empleado</param>
    /// <param name="mail">Correo del empleado</param>
    /// <param name="codeSecurity">Codigo temporal para verificación</param>
    /// <param name="endDateValidation">Fecha de expiración del correo</param>
    /// <param name="userType">Tipo de usuario</param>
    /// <returns></returns>
    public async Task<bool> SendEmailConfirmationAsync(string name, string mail, string codeSecurity, DateTime endDateValidation, UserTypes userType)
    {
        try
        {        
            // Asunto del correo
            string subject = "Confirmación de correo";

            // Leer plantilla de correo
            string dir = Path.GetFullPath("../Infrastructure/Email/Templates/");
            string templatePath = Path.Combine(dir, "ConfirmedEmail.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("La plantilla de correo no fue encontrada.", templatePath);
            }

            string bodyTemplate = await File.ReadAllTextAsync(templatePath);

            // Variables para configuración específica según el tipo de usuario
            string siteType;
            string emailSupport;
            string hostSystem;

            switch (userType)
            {
                case UserTypes.User:
                    siteType = "WebAppBase";
                    emailSupport = _configuration["AppSettings:EmailSupport"];
                    hostSystem = _configuration["AppSettings:ServerAddress"];
                    break;
                default:
                    siteType = "";
                    emailSupport = string.Empty;
                    hostSystem = string.Empty;
                    break;
            }

            // Reemplazo de marcadores en la plantilla
            string emailBody = bodyTemplate
                .Replace("[UserName]", name)
                .Replace("[EmailSupport]", emailSupport)
                .Replace("[Host]", hostSystem)
                .Replace("[VerificationCode]", codeSecurity)
                .Replace("[EndDateValidation]", endDateValidation.ToString("dd/MM/yyyy"))
                .Replace("[EmailUser]", mail)
                .Replace("[SiteType]", siteType)
                .Replace("[Protocolo]", _configuration["AppSettings:Protocolo"]);

            // Envío del correo
            _ = Task.Run(() => _emailService.SendEmailAsync(mail, subject, emailBody));
            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailConfirmationAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Envía el email de restablecer contraseña de un usuario.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="mail"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> SendEmailPasswordResetAsync(string name, string mail, string password)
    {
        try
        {
            // Asunto del correo
            string subject = "Restablecimiento de contraseña del sistema";

            // Leer plantilla de correo
            string dir = Path.GetFullPath("../Infrastructure/Email/Templates/");
            string templatePath = Path.Combine(dir, "ResetPassword.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("La plantilla de correo no fue encontrada.", templatePath);
            }

            string bodyTemplate = await File.ReadAllTextAsync(templatePath);
                   
            // Reemplazo de marcadores en la plantilla
            string emailBody = bodyTemplate
                .Replace("[UserName]", name)
                .Replace("[EmailSupport]", _configuration["AppSettings:EmailSupport"])
                .Replace("[Host]", _configuration["AppSettings:ServerAddress"])
                .Replace("[Password]", password)
                .Replace("[Protocolo]", _configuration["AppSettings:Protocolo"]);

            // Envío del correo
            _ = Task.Run(() => _emailService.SendEmailAsync(mail, subject, emailBody));
            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailPasswordResetAsync), ex);
            return false;
        }
    }

    /// <summary>
    /// Envía el email de notificación al generar un nuevo usuario.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="mail"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> SendEmailNewUserAsync(string name, string mail, string password)
    {
        try
        {
            // Asunto del correo
            string subject = "Bienvenido al sistema";

            // Leer plantilla de correo
            string dir = Path.GetFullPath("../Infrastructure/Email/Templates/");
            string templatePath = Path.Combine(dir, "NewUser.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("La plantilla de correo no fue encontrada.", templatePath);
            }

            string bodyTemplate = await File.ReadAllTextAsync(templatePath);

            // Reemplazo de marcadores en la plantilla
            string emailBody = bodyTemplate
                .Replace("[UserName]", name)
                .Replace("[EmailSupport]", _configuration["AppSettings:EmailSupport"])
                .Replace("[Host]", _configuration["AppSettings:ServerAddress"])
                .Replace("[Password]", password)
                .Replace("[Protocolo]", _configuration["AppSettings:Protocolo"]);

            // Envío del correo
            _ = Task.Run(() => _emailService.SendEmailAsync(mail, subject, emailBody));
            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailNewUserAsync), ex);
            return false;
        }
    }


    /// <summary>
    /// Envía el email cuando el usuario ha olvidado su contraseña y desea recuperarla.
    /// </summary>
    /// <param name="mail"></param>
    /// <param name="callbackUrl"></param>
    /// <returns></returns>
    public async Task<bool> SendEmailForgotPasswordAsync(string mail, string callbackUrl)
    {
        try
        {
            // Asunto del correo
            string subject = "Restablecer contraseña";

            // Leer plantilla de correo
            string dir = Path.GetFullPath("../Infrastructure/Email/Templates/");
            string templatePath = Path.Combine(dir, "ForgotPassword.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("La plantilla de correo no fue encontrada.", templatePath);
            }

            string bodyTemplate = await File.ReadAllTextAsync(templatePath);

            // Reemplazo de marcadores en la plantilla
            string emailBody = bodyTemplate
                .Replace("[callbackUrl]", callbackUrl)
                .Replace("[EmailSupport]", _configuration["AppSettings:EmailSupport"]);

            // Envío del correo
            _ = Task.Run(() => _emailService.SendEmailAsync(mail, subject, emailBody));
            return true;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailForgotPasswordAsync), ex);
            return false;
        }
    }





}
