using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Domain.Enums;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailSender _emailSender;
    private readonly IUserService _userService;
    private readonly ILogService _logService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IEmailSender emailSender,
        IUserService userService,
        ILogService logService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _emailSender = emailSender;
        _userService = userService;
        _logService = logService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        return user?.UserName;
    }

    public async Task<string?> GetUserEmailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.Email;
    }

    public async Task<CurrentUserDto?> GetUserByNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user is null)
            return null;

        var userDto = new CurrentUserDto()
        {
            UserId = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,           
            
        };

        return userDto;
    }

    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var result = await _userManager.IsEmailConfirmedAsync(user);

        return result;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, int employeeId)
    {
        //var user = new ApplicationUser
        //{
        //    UserName = userName,
        //    Email = userName,
        //};

        var user = new ApplicationUser
        {
            Email = userName,
            UserName = userName,
            EmployeeId = employeeId,
            PasswordEndDate = DateTime.Now.AddDays(60),
            EmailConfirmed = true,
            ResetFlag = true,
            Enable = true,
            UserTypeId = (int)UserTypes.User
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool rememberMe)
    {
        // Obtener usuario por email
        var user = await _userManager.FindByEmailAsync(userName);
        if (user == null)
        {
            // Si el usuario no existe, devolver un resultado fallido
            return SignInResult.Failed; // Significa que el usuario no fue encontrado
        }

        // Validar las credenciales del usuario
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            // Si las credenciales no son válidas, devolver un resultado fallido
            return SignInResult.Failed;
        }

        // Si el inicio de sesión es exitoso, persistir la sesión
        await _signInManager.SignInAsync(user, isPersistent: rememberMe);

        // Construir DTO del usuario actual
        var userDto = new CurrentUserDto
        {
            UserId = user.Id,
            Email = user.Email,
            Enable = user.Enable,
            EmployeeId = user.EmployeeId,
            ResetFlag = user.ResetFlag,
        };

        // Recuperar información adicional del usuario
        var currentUser = await _userService.GetCurrentUserInformation(userDto);
        if (currentUser == null)
        {
            // Si no se pudo recuperar la información adicional del usuario, salir
            return SignInResult.Failed; // Podrías también lanzar una excepción si prefieres
        }

        // Serializar la información del usuario y actualizar el claim de UserData
        var serializedUser = JsonConvert.SerializeObject(currentUser);

        // Actualizar el claim de UserData (si no existe, se agrega)
        var existingClaim = (await _userManager.GetClaimsAsync(user))
            .FirstOrDefault(c => c.Type == ClaimTypes.UserData);

        if (existingClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, existingClaim);
        }

        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.UserData, serializedUser));

        // Registro de actividad
        _logService.ActivityLog(user.Id, "Inicio de sesión", "El usuario ha iniciado sesión correctamente.");

        // Retornar el resultado de la autenticación
        return result;
    }
 
    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }


    /// <summary>
    /// Obtenemos la información del usuario desde la cookie.
    /// </summary>
    public CurrentUserDto? GetCurrentUserAsync()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                return new CurrentUserDto();
            }

            //var userDataClaim = user.FindFirst(ClaimTypes.UserData)?.Value;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userData = user.FindFirstValue(ClaimTypes.UserData);

            if (string.IsNullOrEmpty(userData))
            {
                return new CurrentUserDto();
            }

            return JsonConvert.DeserializeObject<CurrentUserDto>(userData);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetCurrentUserAsync), ex);
            return new CurrentUserDto();
        }
    }

    public bool IsUserAuthenticated()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Obtenemos el ID del usuario desde la cookie.
    /// </summary>
    public string GetCurrentUserId()
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                return "";
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return "";
            }

            return userId;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetCurrentUserAsync), ex);
            return "";
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {       
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new ArgumentException($"No se encontró un usuario con el ID '{userId}'.", nameof(userId));
        
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <summary>
    /// Restaura la contraseña de un usuario utilizando su ID, un token de verificación y la nueva contraseña.
    /// </summary>
    /// <param name="userId">ID del usuario cuyo password será reseteado.</param>
    /// <param name="token">Token de verificación proporcionado para el reseteo de contraseña.</param>
    /// <param name="password">Nueva contraseña que se establecerá.</param>
    /// <returns>Resultado de la operación, indicando éxito o error.</returns>
    public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string password)
    {
        // Intentar obtener el usuario por su ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            // Si no se encuentra el usuario, devolver un resultado fallido con un mensaje significativo
            return IdentityResult.Failed(new IdentityError
            {
                Description = $"Usuario con ID '{userId}' no encontrado."
            });
        }

        // Intentar resetear la contraseña
        var result = await _userManager.ResetPasswordAsync(user, token, password);

        // Retornar el resultado de la operación
        return result;
    }

    /// <summary>
    /// Obtiene una lista de los proveedores válidos de autenticación en dos factores para el usuario actual.
    /// </summary>
    /// <returns>
    /// Una lista de nombres de proveedores válidos de autenticación en dos factores si el usuario es encontrado; 
    /// de lo contrario, una lista vacía.
    /// </returns>
    public async Task<IList<string>> GetValidTwoFactorProvidersAsync()
    {
        try
        {
            // Obtener el usuario autenticado para la autenticación en dos factores.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                // Retorna una lista vacía si no se encuentra al usuario.
                return new List<string>();
            }

            // Obtener los proveedores de autenticación en dos factores válidos para el usuario.
            return await _userManager.GetValidTwoFactorProvidersAsync(user);
        }
        catch (Exception ex)
        {
            // Manejo de errores: registra la excepción y lanza una personalizada si es necesario.
            _logService.ErrorLog("GetValidTwoFactorProvidersAsync", ex);
            throw new InvalidOperationException("No se pudo obtener los proveedores válidos de autenticación en dos factores.", ex);
        }
    }

    /// <summary>
    /// Envía un código de autenticación en dos factores (2FA) al usuario que se indique utilizando el proveedor especificado.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="provider">El proveedor de autenticación 2FA (por ejemplo, "Email").</param>
    /// <returns>
    /// Un objeto <see cref="Result"/> indicando el éxito o fracaso de la operación.
    /// </returns>
    public async Task<Result> SendTwoFactorCodeAsync(string userId, string provider)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(new[] { "Usuario no encontrado." });
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return Result.Failure(new[] { "La autenticación en dos pasos no está habilitada para este usuario." });
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);

        switch (provider)
        {
            case "Email":
                await _emailSender.SendEmailAsync(user.Email, "Código de seguridad", $"Tu código es: {code}");
                break;

            //case "Phone":
            //    await _userManager.SendSmsAsync(user.PhoneNumber, $"Tu código de seguridad es: {code}");
            //    break;

            default:
                return Result.Failure(new[] { "Proveedor no soportado." });
        }

        return Result.Success();
    }

    /// <summary>
    /// Envía un código de autenticación en dos factores (2FA) al usuario autenticado utilizando el proveedor especificado.
    /// </summary>
    /// <param name="provider">El proveedor de autenticación 2FA (por ejemplo, "Email").</param>
    /// <returns>
    /// Un objeto <see cref="Result"/> indicando el éxito o fracaso de la operación.
    /// </returns>
    public async Task<Result> SendTwoFactorCodeAsync(string provider)
    {
        try
        {
            // Obtener el usuario autenticado para la autenticación en dos factores.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return Result.Failure(new[] { "Usuario no encontrado." });
            }

            // Verificar si el usuario tiene habilitada la autenticación en dos factores.
            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return Result.Failure(new[] { "La autenticación en dos pasos no está habilitada para este usuario." });
            }

            // Generar el token 2FA para el proveedor especificado.
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
            if (string.IsNullOrWhiteSpace(code))
            {
                var errorMessage = $"El proveedor 2FA '{provider}' no generó un código válido.";
                _logService.ErrorLog(nameof(SendTwoFactorCodeAsync), errorMessage, provider);
                return Result.Failure(new[] { errorMessage });
            }

            // Manejar el envío del código según el proveedor.
            switch (provider)
            {
                case "Email":
                    if (string.IsNullOrWhiteSpace(user.Email))
                    {
                        return Result.Failure(new[] { "El usuario no tiene un correo electrónico válido configurado." });
                    }

                    await _emailSender.SendEmailAsync(user.Email, "Código de seguridad", $"Tu código es: {code}");
                    break;
                default:
                    return Result.Failure(new[] { $"El proveedor '{provider}' no está soportado." });
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error al enviar el código 2FA mediante el proveedor '{provider}'.";
           _logService.ErrorLog(errorMessage, ex);
            return Result.Failure(new[] { errorMessage });
        }
    }

    public async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser)
    {       

        // Intentar resetear la contraseña
        var result = await _signInManager.TwoFactorSignInAsync(provider, code, isPersistent, rememberBrowser);

        // Retornar el resultado de la operación
        return result;
    }



}