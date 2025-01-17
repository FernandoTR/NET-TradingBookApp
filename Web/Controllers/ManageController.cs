using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Application.Common;
using Infrastructure;


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
    private readonly IAccountsService _accountsService;
    private readonly IAccountBalancesService _accountBalancesService;

    public ManageController(IIdentityService identityService, 
                            IEmployeeService employeeService,
                            ILogService logService,
                            IMessageService messageService,
                            IUserEmailService userEmailService,
                            IUserService userService,
                            IQrCodeGeneratorService qrCodeGeneratorService,
                            IAccountsService accountsService,
                            IAccountBalancesService accountBalancesService)
    {
        _identityService = identityService;
        _employeeService = employeeService;
        _logService = logService;
        _messageService = messageService;
        _userEmailService = userEmailService;
        _userService = userService;
        _qrCodeGeneratorService = qrCodeGeneratorService;
        _accountsService = accountsService;
        _accountBalancesService = accountBalancesService;
    }

    // GET: /Manage/Index
    public async Task<ActionResult> Index()
    {     
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
    /// Muestra el balance de la cuenta del usuario.
    /// </summary>
    /// <returns>Vista parcial con el balance de la cuenta del usuario.</returns>
    public async Task<PartialViewResult> Balance()
    {
        var account = await GetUserAccountAsync();

        // Recuperar las cuentas que están asociadas al usuario
        var options = new QueryOptions<Account>
        {
            Where = o => o.UserId == account.AspNetUser.Id,
            OrderBy = o => o.Id,
            Includes = "CatAccountType"
        };
        var accounts = await _accountsService.GetAllAsync(options);

        var model = new BalanceViewModel
        {
           Accounts = accounts.ToList()
        };

        return PartialView("_Balance",model);
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

            await _identityService.RefreshSignInAsync(account.AspNetUser.Email);

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

    /// <summary>
    ///  Funcion para desactivar 2FA
    /// </summary>
    public async Task<IActionResult> DisableTwoFactor()
    {
        try
        {
            // Obtener la cuenta del usuario actual
            var account = await GetUserAccountAsync();

            var result = await _identityService.DisableTwoFactorAsync(account.AspNetUser.Id);
            
            if (!result.Succeeded)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault().Description,
                    notificationType = NotificationType.Error
                });
            }

            await _identityService.RefreshSignInAsync(account.AspNetUser.Email);

            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = "La autenticación de dos factores fue desabilitada correctamente.",
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


    public async Task<IActionResult> WithdrawCash(int id)
    {       
        AccountsViewModel model = null;
        try
        {
            var options = new QueryOptions<Account>
            {
                Includes = "CatAccountType"
            };

            var account = await _accountsService.GetByIdAsync(id, options);
            model = new AccountsViewModel()
            {
                Id = account.Id,
                UserId = account.UserId,
                CatAccountTypeId = account.CatAccountTypeId,
                InitialBalance = account.InitialBalance,
                CurrentBalance = account.CurrentBalance,
                Currency = account.Currency,
                UpdatedAt = account.UpdatedAt,
            };

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
            _logService.ErrorLog($"Controller: Manage , Action: {nameof(WithdrawCash)}", ex);
        }

        return View(model);
    }

    public async Task<IActionResult> AddCash(int id)
    {
        AccountsViewModel model = null;
        try
        {
            var options = new QueryOptions<Account>
            {
                Includes = "CatAccountType"
            };

            var account = await _accountsService.GetByIdAsync(id, options);
            model = new AccountsViewModel()
            {
                Id = account.Id,
                UserId = account.UserId,
                CatAccountTypeId = account.CatAccountTypeId,
                InitialBalance = account.InitialBalance,
                CurrentBalance = account.CurrentBalance,
                Currency = account.Currency,
                UpdatedAt = account.UpdatedAt,
            };

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
            _logService.ErrorLog($"Controller: Manage , Action: {nameof(WithdrawCash)}", ex);
        }

        return View(model);
    }

    /// <summary>
    /// Realiza un retiro de saldo de una cuenta específica.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WithdrawCash([FromBody] AccountsOperationsViewModel model)
    {
        try
        {
            
            if (model.Id <= 0)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "El identificador de la cuenta es obligatorio.",
                    notificationType = NotificationType.Warning
                });
            }

            var isValid = await _accountsService.WithDrawCashAsync(model.Id, Convert.ToDecimal(model.Cash));

            if (!isValid)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "El retiro de saldo no se pudo procesar exitosamente.",
                    notificationType = NotificationType.Error
                });
            }


            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = "El retiro de saldo se procesó correctamente.",
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(WithdrawCash)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
    }

    /// <summary>
    /// Realiza un abono de saldo de una cuenta específica.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCash([FromBody] AccountsOperationsViewModel model)
    {
        try
        {
            if (model.Id <= 0)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "El identificador de la cuenta es obligatorio.",
                    notificationType = NotificationType.Warning
                });
            }

            var isValid = await _accountsService.AddCashAsync(model.Id, Convert.ToDecimal(model.Cash));

            if (!isValid)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "El abono de saldo no se pudo procesar exitosamente.",
                    notificationType = NotificationType.Error
                });
            }


            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = "El abono de saldo se procesó correctamente.",
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Manage, Action: {nameof(AddCash)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
    }



    #region Carga de datos en el DataTable

    // Variables manejadas por DataTable
    public string draw = string.Empty;
    public string start = string.Empty;
    public string length = string.Empty;
    public string sortColumn = string.Empty;
    public string sortColumnDir = string.Empty;
    public string searchValue = string.Empty;

    public int pageSize;
    public int skip;
    public int recordsTotal;

    /// <summary>
    /// Método para cargar los datos en el DataTable de la vista.
    /// Este método maneja la paginación, ordenación y filtrado de los logs por fecha y búsqueda de texto.
    /// </summary>
    /// <param name="fecha1">Fecha de inicio para el filtro de búsqueda.</param>
    /// <param name="fecha2">Fecha de fin para el filtro de búsqueda.</param>
    /// <returns>Un objeto JSON que contiene los datos solicitados por el DataTable.</returns>
    [HttpPost]
    public async Task<ActionResult> JsonDataTable(string fecha1, string fecha2)
    {
        #region Aplicar filtros de búsqueda por fecha

        // Fechas por defecto (últimos 7 días)
        DateTime dateStart = DateTime.Now.AddDays(-7);
        DateTime dateEnd = DateTime.Now;

        // Si se proporcionan fechas, se parsean y se ajustan los valores de inicio y fin
        if (!string.IsNullOrEmpty(fecha1) && !string.IsNullOrEmpty(fecha2))
        {
            fecha1 += " 00:00:00.000";
            fecha2 += " 23:59:59.000";

            // Establecer el rango de fechas
            dateStart = DateTime.Parse(fecha1);
            dateEnd = DateTime.Parse(fecha2);
        }

        #endregion

        var data = new List<AccountBalancesViewModel>();

        try
        {
            // Obtener los parámetros del DataTable enviados en la petición
            var form = await Request.ReadFormAsync(); // Lee el contenido del formulario de la solicitud

            draw = form["draw"].FirstOrDefault();
            start = form["start"].FirstOrDefault();
            length = form["length"].FirstOrDefault();
            sortColumn = form[$"columns[{form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
            sortColumnDir = form["order[0][dir]"].FirstOrDefault();
            searchValue = form["search[value]"].FirstOrDefault();

            // Configurar la paginación
            pageSize = length != null ? Convert.ToInt32(length) : 0;
            skip = start != null ? Convert.ToInt32(start) : 0;
            recordsTotal = 0;

            var userAccount = await GetUserAccountAsync();

            // Obtención de la consulta inicial de logs dentro del rango de fechas
            var query = (await _accountBalancesService
                        .GetAllAccountBalanceByDateRangeAsync(userAccount.AspNetUser.Id, dateStart, dateEnd))
                        .Select(x => new AccountBalancesViewModel
                        {
                            Id = x.Id,
                            AccountId = x.AccountId,
                            OrderId = x.OrderId,
                            Balance = x.Balance,
                            Reference = x.Reference,
                            UpdateAt = x.UpdateAt,
                            AccountTypeName = x.AccountTypeName,
                        });

            // Filtrado por búsqueda (si existe)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => (x.Id + x.AccountId.ToString() + x.OrderId + x.Balance + x.Reference + x.UpdateAt + x.AccountTypeName)
                                          .Contains(searchValue));
            }

            // Ordenación
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = sortColumn switch
                {
                    "id" => sortColumnDir == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                    "accountId" => sortColumnDir == "asc" ? query.OrderBy(x => x.AccountId) : query.OrderByDescending(x => x.AccountId),
                    "orderId" => sortColumnDir == "asc" ? query.OrderBy(x => x.OrderId) : query.OrderByDescending(x => x.OrderId),
                    "balance" => sortColumnDir == "asc" ? query.OrderBy(x => x.Balance) : query.OrderByDescending(x => x.Balance),
                    "updateAt" => sortColumnDir == "asc" ? query.OrderBy(x => x.UpdateAt) : query.OrderByDescending(x => x.UpdateAt),
                    "accountTypeName" => sortColumnDir == "asc" ? query.OrderBy(x => x.AccountTypeName) : query.OrderByDescending(x => x.AccountTypeName),
                    _ => query
                };
            }

            // Obtener el total de registros
            recordsTotal = query.Count();

            // Paginación: saltar los registros ya mostrados y tomar el número de registros indicado
            data = query.Skip(skip).Take(pageSize).ToList();


        }
        catch (Exception ex)
        {
            // Manejo de errores y registro de la excepción
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Manage, Action: JsonDataTable", ex);
        }

        // Retornar los datos al DataTable en formato JSON
        return Json(new
        {
            draw = draw,
            recordsFiltered = recordsTotal,
            recordsTotal = recordsTotal,
            data = data
        });
    }

    #endregion

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

    /// <summary>
    /// Da formato al enlace que se le asignara al QR para que pueda ser leido por las apps
    /// </summary>
    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                             Uri.EscapeDataString("WebAppBase"),
                             email,
                             unformattedKey);
    }

    #endregion

}
