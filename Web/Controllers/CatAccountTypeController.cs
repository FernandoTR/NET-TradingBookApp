using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models.Enums;
using Web.Models;
using Infrastructure;

namespace Web.Controllers;

public class CatAccountTypeController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly ICatAccountTypeService _catAccountTypeService;

    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.CatAccountType;

    public CatAccountTypeController(IIdentityService identityService,
                                    ILogService logService,
                                    IMessageService messageService,
                                    ICatAccountTypeService catAccountTypeService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _catAccountTypeService = catAccountTypeService;
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
    /// <returns>Un objeto JSON que contiene los datos solicitados por el DataTable.</returns>
    [HttpPost]
    public async Task<ActionResult> JsonDataTable()
    {
        var data = new List<CatAccountTypeViewModel>();

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


            // Obtención de la consulta inicial de logs dentro del rango de fechas
            var query = (await _catAccountTypeService
                        .GetAllAsync())
                        .Select(x => new CatAccountTypeViewModel
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            IsActived = x.IsActived,
                            Task = ActionButtonHelper.GenerateActionMenu(new ActionMenuModel
                            {
                                Id = x.Id.ToString(),
                                ActionOptionMenus = new List<ActionOptionMenuModel>
                                {
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Editar",
                                     JavaScriptAction = "showModalForUpdate",
                                 },
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Eliminar",
                                     JavaScriptAction = "showModaltoDelete",
                                 },
                                }

                            })
                        });


            // Filtrado por búsqueda (si existe)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => (x.Id + x.Code + x.Description).Contains(searchValue));
            }

            // Ordenación
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                switch (sortColumn)
                {
                    case "Id":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                        break;
                    case "Code":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code);
                        break;
                    case "Description":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Description) : query.OrderByDescending(x => x.Description);
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
            _logService.ErrorLog($"Controller: CatAccountType, Action: JsonDataTable", ex);
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
            _logService.ErrorLog($"Controller: CatAccountType, Action: {nameof(Index)}", ex);
            return RedirectToAction("index", "Home");
        }

        return View();
    }

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

    [HttpPost]
    public async Task<IActionResult> Save(CatAccountTypeViewModel model)
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
            var catAccountType = new CatAccountType()
            {
                Code = model.Code.Trim(),
                Description = model.Description.Trim(),
                IsActived = true
            };

            if (await _catAccountTypeService.AddAsync(catAccountType))
            {
                notification = NotificationType.Success;
                message = string.Format(_messageService.GetResourceMessage("AccountTypeSuccessfullySaved"), model.Description);
            }
            else
            {
                message = _messageService.GetResourceError("GenericError");
                notification = NotificationType.Error;
            }
        }
        catch (Exception ex)
        {
            message = _messageService.GetResourceError("GenericError");
            notification = NotificationType.Error;
            _logService.ErrorLog($"Controller: CatAccountType, Action: {nameof(Save)}", ex);
        }

        return RedirectToAction("Index", new { notification = notification, message = message });

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
        try
        {
            var catAccountType = await _catAccountTypeService.GetByIdAsync(id);
            if (catAccountType == null)
            {
                return RedirectToAction("Index", new { notification = NotificationType.Error, message = _messageService.GetResourceError("AccountTypeNotFound") });
            }
            var model = new CatAccountTypeViewModel
            {
                Id = catAccountType.Id,
                Code = catAccountType.Code,
                Description = catAccountType.Description,
                IsActived = catAccountType.IsActived
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: CatAccountType, Action: {nameof(Edit)}", ex);
            return RedirectToAction("Index", new { notification = NotificationType.Error, message = _messageService.GetResourceError("GenericError") });
        }
    }


    [HttpPost]
    public async Task<IActionResult> Update(CatAccountTypeViewModel model)
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


        try
        {
            var catAccountType = new CatAccountType()
            {
                Id = model.Id,
                Code = model.Code.Trim(),
                Description = model.Description.Trim(),
                IsActived = (bool)model.IsActived
            };

            if (await _catAccountTypeService.UpdateAsync(catAccountType))
            {
                notification = NotificationType.Success;
                message = string.Format(_messageService.GetResourceMessage("AccountTypeSuccessfullyUpdated"), model.Description);
            }
            else
            {
                message = _messageService.GetResourceError("GenericError");
                notification = NotificationType.Error;
            }
        }
        catch (Exception ex)
        {
            message = _messageService.GetResourceError("GenericError");
            notification = NotificationType.Error;
            _logService.ErrorLog($"Controller: CatAccountType , Action: {nameof(Update)}", ex);
        }


        return RedirectToAction("Index", new { notification = notification, message = message });
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

        try
        {
            bool isDeleted = await _catAccountTypeService.DeleteAsync(id);

            // Devolver un resultado indicando si la eliminación fue exitosa
            return Content(isDeleted.ToString().ToLower());
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: CatAccountType, Action: {nameof(Delete)}", ex);
            return Content("false");
        }
    }



}
