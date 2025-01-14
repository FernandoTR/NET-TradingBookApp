using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models.Enums;
using Web.Models;

namespace Web.Controllers;

public class CatCategoryController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly ICatCategoryService _catCategoryService;

    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.CatCategory;

    public CatCategoryController(IIdentityService identityService,
                               ILogService logService,
                               IMessageService messageService,
                               ICatCategoryService catCategoryService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _catCategoryService = catCategoryService;
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
        var data = new List<CatCategoryViewModel>();

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
            var query = (await _catCategoryService
                        .GetAllAsync())
                        .Select(x => new CatCategoryViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
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
                query = query.Where(x => (x.Id + x.Name ).Contains(searchValue));
            }

            // Ordenación
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                switch (sortColumn)
                {
                    case "Id":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                        break;
                    case "Name":
                        query = sortColumnDir == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
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
            _logService.ErrorLog($"Controller: CatCategory, Action: JsonDataTable", ex);
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
            _logService.ErrorLog($"Controller: CatCategory, Action: {nameof(Index)}", ex);
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
    public async Task<IActionResult> Save(CatCategoryViewModel model)
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
            var figure = new CatCategory()
            {
                Name = model.Name.Trim()
            };

            if (await _catCategoryService.AddAsync(figure))
            {
                notification = NotificationType.Success;
                message = string.Format(_messageService.GetResourceMessage("CategorySuccessfullySaved"), model.Name);
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
            _logService.ErrorLog($"Controller: CatCategory, Action: {nameof(Save)}", ex);
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

        CatCategoryViewModel model = null;
        try
        {
            var category = await _catCategoryService.GetByIdAsync(id);
            model = new CatCategoryViewModel()
            {
                Id = category.Id,
                Name = category.Name
            };

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
            _logService.ErrorLog($"Controller: CatCategory, Action: {nameof(Edit)}", ex);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Update(CatCategoryViewModel model)
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
            var figure = new CatCategory()
            {
                Id = model.Id,
                Name = model.Name.Trim()
            };

            if (await _catCategoryService.UpdateAsync(figure))
            {
                notification = NotificationType.Success;
                message = string.Format(_messageService.GetResourceMessage("CategorySuccessfullyUpdated"), model.Name);
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
            _logService.ErrorLog($"Controller: CatCategory, Action: {nameof(Update)}", ex);
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
            bool isDeleted = await _catCategoryService.DeleteAsync(id);

            // Devolver un resultado indicando si la eliminación fue exitosa
            return Content(isDeleted.ToString().ToLower());
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: CatCategory, Action: {nameof(Delete)}", ex);
            return Content("false");
        }
    }
}
