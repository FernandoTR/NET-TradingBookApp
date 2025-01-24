
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Email.Services;
using Infrastructure.Identity;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using Web.Models;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IIdentityService _identityService;
    private readonly IMessageService _messageService;
    private readonly ILogService _logService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmployeeService _employeeService;
    private readonly IRolesService _rolesService;
    private readonly IUserService _userService;
    private readonly IUserEmailService _userEmailService;
    private readonly IStringUtilitiesService _stringUtilitiesService;
    public AccountController(IConfiguration configuration,
                             IIdentityService identityService, 
                             IMessageService messageService,
                             ILogService logService,
                             IHttpContextAccessor httpContextAccessor,
                             IEmployeeService employeeService,
                             IRolesService rolesService,
                             IUserService userService,
                             IUserEmailService userEmailService,
                             IStringUtilitiesService stringUtilitiesService)
    {
        _configuration = configuration;
        _identityService = identityService;
        _messageService = messageService;
        _logService = logService;
        _httpContextAccessor = httpContextAccessor;
        _employeeService = employeeService;
        _rolesService = rolesService;
        _userService = userService;
        _userEmailService = userEmailService;
        _stringUtilitiesService = stringUtilitiesService;
    }

    // GET: /Account/Login
    [AllowAnonymous]
    public IActionResult SignIn(string returnUrl)
    {
        if (returnUrl == null || !returnUrl.Contains("LogOff"))
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        else
            return RedirectToAction("Index", "Home");
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
    {
        try
        {
            //if (!ModelState.IsValid)
            //{
            //    return View("SignIn", model);
            //}

            // Verifica credenciales
            var result = await _identityService.CheckPasswordSignInAsync(model.Email.Trim(), model.Password);           
            if (result.Succeeded)
            {
                var user = await _identityService.GetUserByNameAsync(model.Email.Trim());

                // Verifica si tiene 2FA habilitado
                if (await _identityService.GetTwoFactorEnabledAsync(user.UserId))
                {
                    var send2FACodeData = new Send2FACodeViewModel { Provider = "", UserId = user.UserId, ReturnUrl = returnUrl, RememberMe = model.RememberMe };
                    TempData["Send2FACodeData"] = JsonConvert.SerializeObject(send2FACodeData);
                    //return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe, user.UserId });
                    return RedirectToAction("SendCode");

                }

                // Inicia sesión
                await _identityService.PasswordSignInAsync(model.Email.Trim(), model.Password, model.RememberMe);                

                // Redirigir en función del resultado
                if (returnUrl == null || !returnUrl.Contains("LogOff"))
                    return RedirectToLocal(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                ViewData[$"notifications.{NotificationType.Warning}"] = "Su cuenta ha sido bloqueda temporalmente, intente de nuevo en 5 minutos.";
                return View("SignIn", model);
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
            }
            else
            {
                ViewData[$"notifications.{NotificationType.Warning}"] = "Intento de inicio de sesión no válido.";
                return View("SignIn", model);
            }

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(Login)}", ex);
            return View("SignIn", model);
        }
    }

    // POST: /Account/LogOff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult LogOff()
    {
        try
        {
            _logService.ActivityLog(_identityService.GetCurrentUserId(), "Cierre de Sesión", "El usuario cerró sesión");

            _identityService.SignOutAsync();
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(LogOff)}", ex);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// GET: /Account/SignUp
    /// </summary>
    /// <param name="id">Identificador del empleado (EmployeeId).</param>
    /// <returns>Vista con el modelo de registro (<see cref="RegisterViewModel"/>).</returns>
    [HttpGet]
    public async Task<IActionResult> SignUp(int id)
    {
        var registerViewModel = new RegisterViewModel();
        try
        {
            // Validar si el usuario ha iniciado sesión.
            var currentUser = _identityService.GetCurrentUserAsync();

            if (!User.Identity.IsAuthenticated || currentUser is null || !currentUser.IsValid)
            {
                return currentUser?.ResetFlag == true
                    ? RedirectToAction("ChangePassword", "Manage")
                    : RedirectToAction("SignIn", "Account");
            }

            // Obtener información del empleado y roles disponibles.
            var employee = (await _employeeService.GetEmployeeByIdAsync(id))?.Email;
            var roleList = (await _rolesService.GetAllAsync())
                .Select(role => new RolUser
                {
                    RolId = role.Id,
                    Name = role.Name
                })
                .ToList();

            // Asignar valores al modelo de vista.
            registerViewModel.EmployeeId = id;
            registerViewModel.Email = employee;
            registerViewModel.Roles = roleList ?? new List<RolUser>();
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(SignUp)}", ex);
        }

        // Retornar la vista con el modelo cargado.
        return View(registerViewModel);
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword() => View();


    /// <summary>
    /// Acción para manejar la solicitud de restablecimiento de contraseña.
    /// </summary>
    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Obtener el usuario asociado al email proporcionado
                var user = await _identityService.GetUserByNameAsync(model.Email);

                // Validar que el usuario exista, que su email esté confirmado y que sea un usuario válido
                if (user == null ||
                    !await _identityService.IsEmailConfirmedAsync(user.UserId) ||
                    !await _userService.IsUserAsync(user.UserId)
                    )
                {
                    // No revelar información sobre la existencia del usuario
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }

                // Generar token para el restablecimiento de contraseña
                string code = await _identityService.GeneratePasswordResetTokenAsync(user.UserId);

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Crear el enlace para el restablecimiento de contraseña
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.UserId, code = code },
                    protocol: Request.Scheme
                );

                // Enviar el correo electrónico al usuario con el enlace de restablecimiento
                await _userEmailService.SendEmailForgotPasswordAsync(user.Email, callbackUrl);

                // Redirigir a la vista de confirmación
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(ForgotPassword)}", ex);
            return View("ResetPassword", model);
        }

        // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
        return View("ResetPassword", model);
    }

    // GET: /Account/ForgotPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ForgotPasswordConfirmation()
    {
        // Acceder a valores del appsettings.json
        ViewBag.emailSupport = _configuration["AppSettings:EmailSupport"];
        return View();
    }


    [AllowAnonymous]
    public async Task<IActionResult> NewPassword(string userId, string code)
    {
        // Validar que los parámetros no sean nulos o vacíos
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            return View("Error");
        }

        // Obtener el correo electrónico del usuario a partir del servicio de identidad
        string? email = await _identityService.GetUserEmailAsync(userId);

        // Verificar si el usuario existe o es válido
        if (email is null)
            return View("Error");


        // Crear el modelo para la vista de restablecimiento de contraseña
        var model = new ResetPasswordViewModel
        {
            Code = code,
            Email = email
        };

        // Regresar la vista con el modelo configurado
        return View(model);
    }


    // POST: /Account/NewPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> NewPassword(ResetPasswordViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar al usuario por su email
            var user = await _identityService.GetUserByNameAsync(model.Email);
            if (user is null)
            {
                // Redirigir sin revelar que el usuario no existe, por razones de seguridad
                return RedirectToAction("NewPasswordConfirmation", "Account");
            }

            // Validar que la nueva contraseña no coincida con las últimas 5 contraseñas del usuario
            if (!await _userService.ValidatePasswordHistoryAsync(user.UserId, model.Password, 5))
            {
                ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("RepeatedPassword");
                return View(model);
            }

            // Intentar restablecer la contraseña del usuario
            var resetResult = await _identityService.ResetPasswordAsync(user.UserId, model.Code, model.Password);
            if (resetResult.Succeeded)
            {
                // Guardar la nueva contraseña en el historial
                await _userService.SavePasswordHistoryAsync(user.UserId, user.PasswordHash);

                // Redirigir al usuario a la confirmación del cambio de contraseña
                return RedirectToAction("NewPasswordConfirmation", "Account");
            }

            AddErrors(resetResult);
            return View();
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(ResetPassword)}", ex);
            return View(model);
        }
    }

    // GET: /Account/NewPasswordConfirmation
    [AllowAnonymous]
    public ActionResult NewPasswordConfirmation()
    {
        // Acceder a valores del appsettings.json
        ViewBag.emailSupport = _configuration["AppSettings:EmailSupport"];
        return View();
    }

    // GET: /Account/SendCode
    [AllowAnonymous]
    public async Task<ActionResult> SendCode()
    {
        try
        {
            // Verificar que TempData contiene la clave
            if (!TempData.ContainsKey("Send2FACodeData"))
            {
                ViewData[$"notifications.{NotificationType.Error}"] = "Datos no válidos o expirados.";
                return View("SignIn");                
            }

            var model = JsonConvert.DeserializeObject<Send2FACodeViewModel>(TempData["Send2FACodeData"].ToString());

            // verificar si existe el usuario
            var user = await _identityService.GetUserEmailAsync(model.UserId);
            if (user == null)
            {
                ViewData[$"notifications.{NotificationType.Warning}"] = "Intento de inicio de sesión no válido.";
                return View("SignIn");
            }

            var twoFactorProviders = (await _identityService.GetValidTwoFactorProvidersAsync(model.UserId)).OrderBy(name => name).ToList();
            if (twoFactorProviders == null)
            {
                return View("Error");
            }
                       
            return View(new SendCodeViewModel { Providers = twoFactorProviders, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe, UserId = model.UserId });
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(SendCode)}", ex);
            return View("Error");
        }
    }

    // POST: /Account/SendCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SendCode(SendCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        try
        {
            string ?sendTo = string.Empty;
           
            if (model.ProviderSelected != "Authenticator")
            {
                // Generar el token y enviarlo
                var result = await _identityService.SendTwoFactorCodeAsync(model.UserId, model.ProviderSelected);
                if (!result.Succeeded)
                {
                    ViewData[$"notifications.{NotificationType.Error}"] = result.Errors[0];
                    return View("SignIn");
                }
                sendTo = model.ProviderSelected == "Email"
                        ? await _identityService.GetUserEmailAsync(model.UserId)
                        : MaskString(input: await _identityService.GetPhoneNumberAsync(model.UserId));
            }

            var send2FACodeViewModel = new Send2FACodeViewModel
            {
                Provider = model.ProviderSelected,
                UserId = model.UserId,
                ReturnUrl = model.ReturnUrl,
                RememberMe = model.RememberMe,
                SendTo = sendTo,
            };
            
            TempData["Send2FACodeData"] = JsonConvert.SerializeObject(send2FACodeViewModel);

            return RedirectToAction("Send2FACode");

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(SendCode)}", ex);
            return View();
        }
    }   


    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
        string message = string.Empty;
        NotificationType notification = NotificationType.Information;

        try
        {
            // Validar si el usuario ha iniciado sesión.
            var currentUser = _identityService.GetCurrentUserAsync();

            if (!User.Identity.IsAuthenticated || currentUser is null || !currentUser.IsValid)
            {
                return currentUser?.ResetFlag == true
                    ? RedirectToAction("ChangePassword", "Manage")
                    : RedirectToAction("SignIn", "Account");
            }


            // Validar el modelo y los datos esenciales.
            if (!ModelState.IsValid || model.EmployeeId <= 0 || string.IsNullOrWhiteSpace(model.Email) || !model.Roles.Any(x => x.IsCheked))
            {
                message = _messageService.GetResourceError("ProblemGeneratingUser");
                notification = NotificationType.Error;
                return RedirectToAction("../Employees/Index", new {  notification, message });
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                EmployeeId = model.EmployeeId,
                PasswordEndDate = DateTime.Now.AddDays(60),
                EmailConfirmed = true,
                ResetFlag = true,
                Enable = true,
                UserTypeId = (int)UserTypes.User
            };

            // Generar contraseña temporal.
            string password = _stringUtilitiesService.GetRandomPassword(8);

            // Genera el usuario
            var createUserResult = await _identityService.CreateUserAsync(user.UserName, password, model.EmployeeId);

            if (!createUserResult.Result.Succeeded)
            {
                message = createUserResult.Result.Errors.FirstOrDefault() ?? _messageService.GetResourceError("GenericError");
                notification = NotificationType.Error;
                return RedirectToAction("../Employees/Index", new { notification, message });
            }

            // Registrar actividad de creación de usuario.
            var userId = createUserResult.UserId;
            _logService.ActivityLog(userId, "Creación de Usuario", $"Usuario creado: {user.UserName}");

            // Obtener usuario creado.
            var createdUser = await _identityService.GetUserByNameAsync(user.Email);

            // Asignar roles al usuario.
            var userRoles = model.Roles
                .Where(r => r.IsCheked)
                .Select(r => new AspNetUserRole { UserId = userId, RoleId = r.RolId })
                .ToList();

            if (await _userService.UpdateAsync(new AspNetUser { Id = userId }, userRoles))
            {
                _logService.ActivityLog(userId, "Asignación de Roles", $"Roles asignados al usuario: {user.UserName}");
            }

            // Guardar historial de contraseñas y enviar email con contraseña temporal.
            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            await _userService.SavePasswordHistoryAsync(userId, createdUser.PasswordHash);

            if (await _userEmailService.SendEmailNewUserAsync(employee.Name, model.Email, password))
            {
                _logService.ActivityLog(userId, "Envío de correo", $"Correo con contraseña enviado a: {user.UserName}");
            }

            message = _messageService.GetResourceMessage("UserGenerated");
            notification = NotificationType.Success;         
            

        }
        catch (Exception ex)
        {           
            _logService.ErrorLog($"Controller: Account, Action: {nameof(Register)}", ex);
            message = _messageService.GetResourceError("GenericError");
            notification = NotificationType.Error;
        }

        return RedirectToAction("Index", "Employees", new { notification, message });
    }


    // GET: /Account/ConfirmEmail
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(string userId, string code)
    {
        try
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await _identityService.IsEmailConfirmedAsync(userId);
            return View(result ? "ConfirmEmail" : "Error");
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Account, Action: {nameof(ConfirmEmail)}", ex);
            return View("Error");
        }
    }

    // GET: /Account/Send2FACode
    [AllowAnonymous]
    public async Task<IActionResult> Send2FACode()
    {
        // Verificar que TempData contiene la clave
        if (!TempData.ContainsKey("Send2FACodeData"))
        {
            ViewData[$"notifications.{NotificationType.Error}"] = "Datos no válidos o expirados.";
            return View("SignIn");
        }

        var model = JsonConvert.DeserializeObject<Send2FACodeViewModel>(TempData["Send2FACodeData"].ToString());

        // verificar si existe el usuario
        var user = await _identityService.GetUserEmailAsync(model.UserId);
        if (user == null)
        {
            ViewData[$"notifications.{NotificationType.Warning}"] = "Intento de inicio de sesión no válido.";
            return View("SignIn");
        }

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify2FACode(Send2FACodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var isValid = await _identityService.VerifyTwoFactorTokenAsync(model.UserId, model.Provider, model.Code);

        if (isValid)
        {
            // Código válido
            await _identityService.TwoFactorSignInAsync(model.UserId, model.RememberMe);
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // Código inválido
            ViewData[$"notifications.{NotificationType.Error}"] = "El código es incorrecto.";
            return View("Send2FACode", model);
        }

        


    }

    #region Funciones Auxiliares

    // Función auxiliar para manejar las redirecciones seguras
    private ActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = error;
        }
    }

    static string MaskString(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= 4)
        {
            return input; // Si la longitud es menor o igual a 4, devuelve la cadena original
        }

        int maskLength = input.Length - 4;
        string maskedPart = new string('*', maskLength); // Crea los asteriscos
        string lastFourDigits = input.Substring(maskLength); // Obtiene los últimos 4 caracteres

        return maskedPart + lastFourDigits;
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult SetTempData([FromBody] Send2FACodeViewModel model)
    {    
        TempData["Send2FACodeData"] = JsonConvert.SerializeObject(model);
        return Json(new { success = true, redirectUrl = Url.Action("SendCode") });
    }


    #endregion

}
