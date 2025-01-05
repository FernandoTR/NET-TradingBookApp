using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using Web.Helpers;
using Web.Models;
using Web.Models.Enums;


namespace Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IRolesService _rolesService;
    private readonly IMenuService _menuService;

    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.Roles;

    public RolesController(IIdentityService identityService,
                          ILogService logService,
                          IMessageService messageService,
                          IRolesService rolesService,
                          IMenuService menuService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _rolesService = rolesService;
        _menuService = menuService;
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
        var data = new List<RolesViewModel>();

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
            var query = (await _rolesService
                        .GetAllAsync())
                        .OrderBy(x => x.Name)
                        .Select(x => new RolesViewModel
                        {
                            RolId = x.Id,
                            NameRoles = x.Name,
                            Task = ActionButtonHelper.GenerateActionMenu( new ActionMenuModel
                            {
                                Id = x.Id.ToString(),
                                ActionOptionMenus = new List<ActionOptionMenuModel> 
                                { 
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Editar",
                                     JavaScriptAction = "showModalForUpdate",
                                 }
                                }
                            }
                            )
                        });

            // Filtrado por búsqueda (si existe)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => (x.RolId + x.NameRoles).Contains(searchValue));
            }

            // Ordenación
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = sortColumn switch
                {
                    "RolId" => sortColumnDir == "asc" ? query.OrderBy(x => x.RolId) : query.OrderByDescending(x => x.RolId),
                    "NameRoles" => sortColumnDir == "asc" ? query.OrderBy(x => x.NameRoles) : query.OrderByDescending(x => x.NameRoles),
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
            _logService.ErrorLog($"Controller: Roles, Action: JsonDataTable", ex);
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

    #region Carga de datos en el arbol de checkboxs
    /// <summary>
    /// Método para cargar los checkbox de permisos en forma de arbol.
    /// </summary>
    /// <param name="rolId">Identificador del rol para obtener sus permisos asociados.</param>
    /// <returns></returns>
    public async Task<IActionResult> GetPermissions(string rolId)
    {
        var accessMenuList = new List<AccessMenuViewModel>();
        var records = new List<AccessMenuTreeViewModel>();

        try
        {
            // Obtenemos todas las opciones de menú disponibles para la asignación de permisos
            accessMenuList = (await _menuService.GetAllAsync())
                .Select(x => new AccessMenuViewModel
                {
                    MenuId = x.Id,
                    Name = x.Name,
                    ParentMenuId = x.ParentMenuId,
                    Position = x.Position,
                    IsSeleted = false // Inicializamos el estado de selección en falso
                }).ToList();

            // Si se proporciona un rol válido, recuperamos las opciones que tiene habilitadas
            if (!string.IsNullOrEmpty(rolId))
            {
                var rol = await _rolesService.FindAsync(rolId);

                if (rol != null)
                {
                    // Marcamos las opciones habilitadas según los permisos del rol
                    foreach (var permiso in accessMenuList)
                    {
                        var accessMenu = rol.AccessMenus.FirstOrDefault(item => item.MenuId == permiso.MenuId);
                        if (accessMenu != null)
                        {
                            permiso.IsSeleted = true; // Marcamos el permiso como seleccionado
                        }
                    }
                }
            }

            // Cargamos los registros del menú, estructurados en un formato jerárquico
            records = accessMenuList
                .Where(x => x.ParentMenuId == null) // Obtenemos los elementos principales (sin padre)
                .OrderBy(x => x.Position) // Ordenamos según la posición
                .Select(x => new AccessMenuTreeViewModel
                {
                    id = x.MenuId,
                    text = x.Name,
                    State = new JsTreeNodeState { Selected = x.IsSeleted }, // Establecemos si está seleccionado según los permisos del rol
                    children = GetChildren(accessMenuList, x.MenuId), // Obtenemos los elementos hijos del menú
                }).ToList();           
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Roles, Action: {nameof(GetPermissions)}", ex);
        }

        return Json(records);
    }

    /// <summary>
    /// Recupera los hijos de una opción de menú
    /// </summary>
    /// <param name="accessMenuList"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    private List<AccessMenuTreeViewModel> GetChildren(List<AccessMenuViewModel> accessMenuList, int parentId)
    {
        return accessMenuList.Where(x => x.ParentMenuId == parentId).OrderBy(x => x.Position)
            .Select(x => new AccessMenuTreeViewModel
            {
                id = x.MenuId,
                text = x.Name,
                State = new JsTreeNodeState { Selected = x.IsSeleted },
                children = GetChildren(accessMenuList, x.MenuId)
            }).ToList();
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
            _logService.ErrorLog($"Controller: Roles, Action: {nameof(Index)}", ex);
            return RedirectToAction("index", "Home");
        }

        return View();
    }

    /// <summary>
    /// Enviar el formulario para una nuevo rol 
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

        var model = new RolesViewModel();
        return View(model);
    }

    /// <summary>
    /// Metodo para mostrar el formulario con las peticiones
    /// </summary>
    /// <returns>Regresa con el formulario precargado de los roles</returns>
    public async Task<IActionResult> Edit(string id)
    {
        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }

        // Crear el modelo para la vista.
        var rolViewModel = new RolesViewModel();       

        try
        {
            // Buscar el rol por su ID.
            var rol = await _rolesService.FindAsync(id);

            if (rol == null)
            {
                // Manejo de caso donde no se encuentra el rol.
                var message = _messageService.GetResourceError("RoleIdNotFound");
                return RedirectToAction("Index", "Roles", new
                {
                    notification = NotificationType.Error,
                    message = message
                });
            }

            // Mapear los datos del rol al modelo de vista.
            rolViewModel.RolId = rol.Id;
            rolViewModel.NameRoles = rol.Name;

            return View(rolViewModel);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Roles, Action: {nameof(Edit)}", ex);
            var message = _messageService.GetResourceError("GenericError");
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = message
            });
        }

       
    }

    /// <summary>
    /// Guarda un nuevo rol y sus accesos asociados.
    /// </summary>
    /// <param name="model">Modelo con la información del rol y sus accesos.</param>
    /// <returns>Redirige a la página de inicio con una notificación.</returns>
    [HttpPost]
    public async Task<IActionResult> Save(RolesViewModel model)
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
            // Crear el rol
            var role = new AspNetRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.NameRoles?.Trim()
            };

            // Crear lista de accesos asociados al rol
            var accessMenuList = new List<AccessMenu>();
            if (!string.IsNullOrWhiteSpace(model.listAccessString))
            {
                //Recuperar las opciones seleccionadas
                model.listAccess = model.listAccessString.Split(',').Select(int.Parse)?.ToList();

                accessMenuList = (from menuId in model.listAccess
                                  select new AccessMenu()
                                  {
                                      MenuId = menuId
                                  }).ToList();
            }

            // Guardar el rol y accesos
            if (await _rolesService.AddAsync(role, accessMenuList))
            {
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Success,
                    message = string.Format(_messageService.GetResourceMessage("RolSuccessfullySaved"), model.NameRoles)
                });
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
            // Loggear el error
            _logService.ErrorLog($"Controller: Roles, Action: {nameof(Save)}", ex);

            // Redirigir con notificación de error
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = _messageService.GetResourceError("GenericError")
            });
        }
    }

    /// <summary>
    /// Actualiza un rol y sus accesos asociados.
    /// </summary>
    /// <param name="model">Modelo con la información del rol y sus accesos.</param>
    /// <returns>Redirige a la página de inicio con una notificación.</returns>
    [HttpPost]
    public async Task<IActionResult> Update(RolesViewModel model)
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
            // Crear el rol
            var role = new AspNetRole
            {
                Id = model.RolId,
                Name = model.NameRoles?.Trim()
            };

            // Crear lista de accesos asociados al rol
            var accessMenuList = new List<AccessMenu>();
            if (!string.IsNullOrWhiteSpace(model.listAccessString))
            {
                //Recuperar las opciones seleccionadas
                model.listAccess = model.listAccessString.Split(',').Select(int.Parse)?.ToList();

                accessMenuList = (from menuId in model.listAccess
                                  select new AccessMenu()
                                  {
                                      MenuId = menuId,
                                      RolId = model.RolId
                                  }).ToList();
            }

            // Guardar el rol y accesos
            if (await _rolesService.UpdateAsync(role, accessMenuList))
            {
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Success,
                    message = string.Format(_messageService.GetResourceMessage("RolSuccessfullyUpdated"), model.NameRoles)
                });
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
            // Loggear el error
            _logService.ErrorLog($"Controller: Roles, Action: {nameof(Update)}", ex);

            // Redirigir con notificación de error
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = _messageService.GetResourceError("GenericError")
            });
        }
    }




}
