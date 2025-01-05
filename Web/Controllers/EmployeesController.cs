using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure.Logging;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using Web.Helpers;
using Web.Models;
using Web.Models.Enums;
using Infrastructure.Email.Services;

namespace Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IEmployeeService _employeeService;
    private readonly IStringUtilitiesService _stringUtilitiesService;
    private readonly IUserEmailService _userEmailService;
    private readonly IUserService _userService;

    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.Employees;

    public EmployeesController(IIdentityService identityService,
                          ILogService logService,
                          IMessageService messageService,
                          IEmployeeService employeeService,
                          IStringUtilitiesService stringUtilitiesService,
                          IUserEmailService userEmailService,
                          IUserService userService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _employeeService = employeeService;
        _stringUtilitiesService = stringUtilitiesService;
        _userEmailService = userEmailService;
        _userService = userService;
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
    /// Este método maneja la paginación, ordenación y filtrado de los datos.
    /// </summary>
    /// <returns>Un objeto JSON que contiene los datos solicitados por el DataTable.</returns>
    [HttpPost]
    public async Task<ActionResult> JsonDataTable()
    {
        var data = new List<EmployeesViewModel>();
        bool showInactive = false;

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


            // Obtención de la consulta inicial de roles
            var query = (await _employeeService
                        .GetAllAsync())
                        .Select(x => new EmployeesViewModel
                        {
                            EmployeeId = x.Employee.Id,
                            Number = x.Employee.Number,
                            Name = x.Employee.Name,
                            LastName = x.Employee.LastName,
                            StatusEmployeeId = x.Employee.StatusEmployeeId,
                            StatusName = x.Employee.StatusEmployee.Name,
                            ConfirmedEmail = x.Employee.ConfirmedEmail,
                            HaveUser = x.AspNetUser?.UserName != null,
                            Task = ActionButtonHelper.GenerateActionMenu(new ActionMenuModel
                            {
                                Id = x.Employee.Id.ToString(),
                                ActionOptionMenus = new List<ActionOptionMenuModel>
                                {
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Editar",
                                     JavaScriptAction = "showModalForUpdate",
                                 },
                                 new ActionOptionMenuModel
                                 {
                                     Title = (x.Employee.StatusEmployeeId == 1 ? "Deshabilitar": "Habilitar"),
                                     JavaScriptAction = "toDeleteEmployee",
                                 },
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Generar usuario",
                                     JavaScriptAction = (x.Employee.ConfirmedEmail && x.AspNetUser?.UserName == null ? "showModalForCreateUser" : string.Empty),
                                 },
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Reenviar correo de verificación",
                                     JavaScriptAction = (!x.Employee.ConfirmedEmail ? "showModalForResendMail" : string.Empty),
                                 }
                                }
                            }
                            )
                        });

            ///custom filter for column 
            for (int i = 0; i < 7; i++)
            {
                var field = "Columns[" + i + "][search][value]";
                if (!string.IsNullOrEmpty(form[field].FirstOrDefault()))
                {
                    var value = form[field].FirstOrDefault();
                    switch (i)
                    {
                        case 0:
                            query = query.Where(x => x.Number.Contains(value));
                            break;
                        case 1:
                            query = query.Where(x => x.Name.Contains(value));
                            break;
                        case 2:
                            query = query.Where(x => x.LastName.Contains(value));
                            break;
                        case 3:
                            if (!value.Equals("TODOS"))
                            {
                                query = query.Where(x => x.StatusName.Contains(value));
                                showInactive = value.Equals("INACTIVO");
                            }
                            else
                            {
                                showInactive = true;
                            }
                            break;
                        case 4:
                            if (!value.Equals("TODOS"))
                            {
                                bool valueBoolean = Convert.ToBoolean(value);
                                query = query.Where(x => x.ConfirmedEmail == valueBoolean);
                            }
                            break;
                        case 5:
                            if (!value.Equals("TODOS"))
                            {
                                bool valueBoolean = Convert.ToBoolean(value);
                                query = query.Where(x => x.HaveUser == valueBoolean);
                            }
                            break;
                    }
                }
            }
            // Filtrado por búsqueda (si existe)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => (x.Number + x.Name + x.LastName + x.StatusName).Contains(searchValue));
            }

            // Ordenación
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                switch (sortColumn)
                {
                    case "Number":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Number) : query.OrderByDescending(x => x.Number);
                        break;
                    case "Name":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                        break;
                    case "LastName":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.LastName) : query.OrderByDescending(x => x.LastName);
                        break;
                    case "StatusName":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.StatusName) : query.OrderByDescending(x => x.StatusName);
                        break;
                    case "HaveUser":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.HaveUser) : query.OrderByDescending(x => x.HaveUser);
                        break;
                    case "Task":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Task) : query.OrderByDescending(x => x.Task);
                        break;
                }
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
            _logService.ErrorLog($"Controller: Employees, Action: JsonDataTable", ex);
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

    public IActionResult Index(NotificationType? notification, string message)
    {
        try
        {
            // Validamos si el usuario no ha iniciado sesión para redireccionarlo al login.
            var currentUser = _identityService.GetCurrentUserAsync();

            if (!User.Identity.IsAuthenticated || currentUser == null || !currentUser.IsValid)
            {
                return currentUser?.ResetFlag == true
                    ? RedirectToAction("ChangePassword", "Manage")
                    : RedirectToAction("SignIn", "Account");
            }


            //Verificar si tiene el permiso
            if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
            {
                return RedirectToAction("Index", "Home");
            }


            // Si hay un mensaje y una notificación, lo agregamos al ViewData
            if (!string.IsNullOrEmpty(message) && notification != null)
            {
                ViewData[$"notifications.{notification}"] = message;
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Logs, Action: {nameof(Index)}", ex);
            return RedirectToAction("index", "Home");
        }

        return View();
    }

    /// <summary>
    /// Enviar el formulario para un nuevo empleado 
    /// </summary>
    /// <returns>Vista New </returns>
    public IActionResult New()
    {
        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }


        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }

        EmployeesViewModel model = null;
        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            var employeeViewModel = new EmployeesViewModel()
            {
                EmployeeId = employee.Id,
                AuthorId = employee.AuthorId,
                CreationDate = employee.CreationDate,
                AlterDate = employee.AlterDate,
                AlterAuthorId = employee.AlterAuthorId,
                Location = employee.Location,
                Number = employee.Number.Trim(),
                Name = employee.Name.Trim(),
                LastName = employee.LastName.Trim(),
                Email = employee.Email.Trim(),
                ConfirmationHash = employee.ConfirmationHash,
                ConfirmationHashEndDate = employee.ConfirmationHashEndDate,
                ConfirmedEmail = employee.ConfirmedEmail,
                StatusEmployeeId = employee.StatusEmployeeId,
                EmployeeNumberTemp = employee.Number.Trim(),
                EmailTemp = employee.Email.Trim()
            };
            model = employeeViewModel;

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(Edit)}", ex);            
        }

        return View(model);
    }



    [HttpPost]
    public async Task<IActionResult> Save(EmployeesViewModel model)
    {
        string message = string.Empty;
        NotificationType notification = NotificationType.Information;

        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }


       
        try
        {
            var employee = new Employee()
            {
                Number = model.Number.Trim(),
                Name = model.Name.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                ConfirmedEmail = false,
                ConfirmationHash = _stringUtilitiesService.GetRandomHash(),
                ConfirmationHashEndDate = DateTime.Now.AddDays(7),
                StatusEmployeeId = (int)EmployeesStatus.Active,
                CreationDate = DateTime.Now,
                AuthorId = _identityService.GetCurrentUserId(),
                Location = model.Location

            };

            if (await _employeeService.AddEmployeeAsync(employee))
            {
                notification = NotificationType.Success;
                message = string.Format(_messageService.GetResourceMessage("EmployeeSuccessfullySaved"), model.Number);
                bool mailsent = await _userEmailService.SendEmailConfirmationAsync(employee.Name, employee.Email, employee.ConfirmationHash, employee.ConfirmationHashEndDate ?? DateTime.Now, UserTypes.User);

                if (!mailsent)
                {
                    var ex = new Exception(_messageService.GetResourceError("ErrorSendingMail"));
                    _logService.ErrorLog("EmployeesController.Save", ex);
                }
                else
                {
                    _logService.ActivityLog(currentUser.UserId, "Correo enviado", $"Se envió el correo de confirmación de correo del empleado número: {employee.Number} al correo: {employee.Email}");
                }
            }
            else
            {
                // Redirigir con notificación de error
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Error,
                    message = _messageService.GetResourceError("GenericError")
                });
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(Save)}", ex);

            // Redirigir con notificación de error
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = _messageService.GetResourceError("GenericError")
            });
        }
        
        return RedirectToAction("Index", new { notification = notification, message = message });

    }

    [HttpPost]
    public async Task<IActionResult> Update(EmployeesViewModel model)
    {
        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }

        string message = string.Empty;
        NotificationType notification = NotificationType.Information;

        if (ModelState.IsValid)
        {
            try
            {
                var employee = new Employee()
                {
                    Id = model.EmployeeId,
                    CreationDate = model.CreationDate,
                    AuthorId = model.AuthorId,
                    Number = model.Number.Trim(),
                    Name = model.Name.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = model.Email.Trim(),
                    StatusEmployeeId = model.StatusEmployeeId,
                    AlterDate = DateTime.Now,
                    AlterAuthorId = currentUser.UserId,
                    Location = model.Location,
                    ConfirmationHash = model.Email.Trim().Equals(model.EmailTemp.Trim()) ? model.ConfirmationHash : _stringUtilitiesService.GetRandomHash(),
                    ConfirmationHashEndDate = model.Email.Trim().Equals(model.EmailTemp.Trim()) ? model.ConfirmationHashEndDate : DateTime.Now.AddDays(7),
                    ConfirmedEmail = model.Email.Trim().Equals(model.EmailTemp.Trim()) && model.ConfirmedEmail
                };

                if (await _employeeService.UpdateEmployeeAsync(employee))
                {
                    notification = NotificationType.Success;
                    message = string.Format(_messageService.GetResourceMessage("EmployeeSuccessfullyUpdated"), model.Number);

                    //Si se cambió el correo se manda la solicitud de confirmación
                    if (!model.Email.Trim().Equals(model.EmailTemp.Trim()))
                    {
                        // Update email del usuario
                        var user = await _userService.FindByEmployeeAsync(employee.Id);
                        if (user != null)
                        {
                            user.Email = model.Email;
                            user.EmailConfirmed = false; // desconfirmamos el mail 
                            await _userService.UpdateAsync(user); // actualizamos el correo 
                        }

                        bool mailsent = await _userEmailService.SendEmailConfirmationAsync(employee.Name, employee.Email, employee.ConfirmationHash, employee.ConfirmationHashEndDate ?? DateTime.Now, UserTypes.User);

                        if (!mailsent)
                        {
                            var ex = new Exception(_messageService.GetResourceError("ErrorSendingMail"));
                            _logService.ErrorLog("EmployeesController.Update", ex);
                        }

                        message += ", Se envio un correo para validar el nuevo correo";
                    }
                }
                else
                {
                    return RedirectToAction("Index", new
                    {
                        notification = NotificationType.Error,
                        message = _messageService.GetResourceError("GenericError")
                    });
                }
            }
            catch (Exception ex)
            {
                _logService.ErrorLog($"Controller: Employees, Action: {nameof(Update)}", ex);

                // Redirigir con notificación de error
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Error,
                    message = _messageService.GetResourceError("GenericError")
                });
            }
        }

        return RedirectToAction("Index", new { notification = notification, message = message });
    }

    /// <summary>
    /// Metodo para reenviar el correo
    /// </summary>
    /// <param name="id">Id del empleado que se enviara el correo</param>
    /// <returns>regresa true o false segun el estado de la operación</returns>
    [HttpPost]
    public async Task<IActionResult> ForwardMail([FromBody] int id)
    {
        string result = "false";

        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee != null)
            {
                employee.ConfirmationHash = _stringUtilitiesService.GetRandomHash();
                employee.ConfirmationHashEndDate = DateTime.Now.AddDays(7);
                employee.ConfirmedEmail = false;

                if (await _employeeService.UpdateEmployeeAsync(employee))
                {
                    bool mailsent = await _userEmailService.SendEmailConfirmationAsync(employee.Name, employee.Email, employee.ConfirmationHash, employee.ConfirmationHashEndDate ?? DateTime.Now, UserTypes.User);

                    if (!mailsent)
                    {
                        var ex = new Exception(_messageService.GetResourceError("ErrorSendingMail"));
                        _logService.ErrorLog($"Controller: Employees, Action: {nameof(ForwardMail)}", ex);
                    }
                    else
                    {
                        _logService.ActivityLog(_identityService.GetCurrentUserId(), "Reenvío de correo", $"Se reenvió el correo de confirmación de correo del empleado número: {employee.Number} al correo: {employee.Email}");
                        result = "true";
                    }

                }

            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(ForwardMail)}", ex);
        }

        return Content(result);
    }

   
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }

        string result;
        try
        {
            if (await _employeeService.DeleteAsync(id))
            {
                result = "true";

                #region Se registra el Log del evento (Actualizacion de estatus del empleado)
                //se registra el evento de la actualizacion del estatus del empleado en la tabla de eventos
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                _logService.ActivityLog(_identityService.GetCurrentUserId(), "Cambio en el Estatus del Empleado", $"Se cambio el estatus del Empleado: {employee.Name}");
                #endregion
            }
            else
            {
                result = "false";
            }

        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(Delete)}", ex);
            result = "false";
        }
        return Content(result);
    }

    /// <summary>
    /// Metodo para buscar si ya existe el Numero de empleado
    /// </summary>
    /// <param name="employeedNumber"></param>
    /// <param name="employeedNumberTemp"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CheckDuplicateKey(string employeedNumber, string employeedNumberTemp)
    {
        bool isDuplicate;

        try
        {
            isDuplicate = await _employeeService.CheckNumberExistsAsync(employeedNumber, employeedNumberTemp);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(CheckDuplicateKey)}", ex);
            isDuplicate = false;
        }

        return Json(new { valid = !isDuplicate });
    }

    /// <summary>
    /// Busca si existe el empleado a partir del correo que esta utilizando
    /// </summary>
    /// <param name="email">Correo del empleado</param>
    /// <param name="emailTemp">Correo del empleado anterior</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CheckDuplicateEmail(string email, string emailTemp)
    {
        bool isDuplicate;

        try
        {
            isDuplicate = await _employeeService.CheckEmailExistsAsync(email, emailTemp);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(CheckDuplicateEmail)}", ex);
            isDuplicate = false;
        }

        return Json(new { valid = !isDuplicate });
    }




}
