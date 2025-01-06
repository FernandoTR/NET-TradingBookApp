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

namespace Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly ILogService _logService;
        private readonly IMessageService _messageService;
        private readonly IEmployeeService _employeeService;
        private readonly IUserService _userService;
        private readonly IRolesService _rolesService;

        // Identificador del permiso 
        private static int permissionNumber = (int)Permissions.Users;

        public UsersController(IIdentityService identityService,
                          ILogService logService,
                          IMessageService messageService,
                          IEmployeeService employeeService,
                          IUserService userService,
                          IRolesService rolesService)
        {
            _identityService = identityService;
            _logService = logService;
            _messageService = messageService;
            _employeeService = employeeService;
            _userService = userService;
            _rolesService = rolesService;
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
            var data = new List<UsersListViewModel>();
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
                            .Where(x => x.Employee.StatusEmployeeId == 1)
                            .Select(x => new UsersListViewModel
                            {
                                Id = x.AspNetUser.Id,
                                EmployeeId = x.Employee.Id,
                                Email = x.Employee.Email,
                                Enable = x.AspNetUser.Enable,
                                PasswordEndDate = x.AspNetUser.PasswordEndDate,
                                AccesFailedCount = x.AspNetUser.AccessFailedCount,
                                Name = string.Join(", ", x.AspNetUser.AspNetUserRoles.Select(role => role.Role.Name)),
                                EmailConfirmed = x.AspNetUser.EmailConfirmed,
                                StatusEmployeeId = x.Employee.StatusEmployeeId,
                                Status = (x.AspNetUser.Enable == true ? "Activo" : "Inactivo"),
                                ResetFlag = x.AspNetUser.ResetFlag,
                                Task = ActionButtonHelper.GenerateActionMenu(new ActionMenuModel
                                {
                                    Id = x.AspNetUser.Id,
                                    ActionOptionMenus = new List<ActionOptionMenuModel>
                                    {
                                         new ActionOptionMenuModel
                                         {
                                             Title = "Editar",
                                             JavaScriptAction = (x.AspNetUser.UserTypeId == 1 ? "showModalForModifyRoles" : string.Empty),
                                         },
                                         new ActionOptionMenuModel
                                         {
                                             Title = (x.AspNetUser.Enable ? "Deshabilitar": "Habilitar"),
                                             JavaScriptAction = "toDeleteUser",
                                         }                                 
                                    }
                                }
                                )
                            });


                if (searchValue != string.Empty)
                {
                    if (searchValue.ToLower() == "inactivo")
                    {
                        query = query.Where(x => (x.Email + x.Name + x.PasswordEndDate.ToString()).Contains("") && x.Enable == false);
                    }
                    else
                    {
                        query = query.Where(x => (x.Email + x.Name + x.PasswordEndDate.ToString()).Contains(searchValue));
                    }
                }

                // Ordenación
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    switch (sortColumn)
                    {
                        case "Email":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
                            break;
                        case "EmailConfirmed":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.EmailConfirmed) : query.OrderByDescending(x => x.EmailConfirmed);
                            break;
                        case "Enable":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Enable) : query.OrderByDescending(x => x.Enable);
                            break;
                        case "AccesFailedCount":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.AccesFailedCount) : query.OrderByDescending(x => x.AccesFailedCount);
                            break;
                        case "Name":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                            break;
                        case "FormatedDate":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.FormatedDate) : query.OrderByDescending(x => x.FormatedDate);
                            break;
                        case "ResetFlag":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.ResetFlag) : query.OrderByDescending(x => x.ResetFlag);
                            break;
                        case "PasswordEndDate":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.PasswordEndDate) : query.OrderByDescending(x => x.PasswordEndDate);
                            break;
                        case "Status":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Enable) : query.OrderByDescending(x => x.Enable);
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
                _logService.ErrorLog($"Controller: Users, Action: JsonDataTable", ex);
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
        /// Método para cargar los perfiles asociados a un usuario
        /// </summary>
        /// <param name="id">El identificador del usuario a editar.</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(string id)
        {
            // Recuperar el usuario logueado
            var currentUser = _identityService.GetCurrentUserAsync();

            // Validar permisos del usuario.
            if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
            {
                return RedirectToAction("Index", "Home");
            }

            // Buscar información del usuario y sus roles asociados.
            var userInfo = await _userService.FindUserRolesAsync(id);

            if (userInfo == null)
            {
                // Manejar el caso donde el usuario no existe.
                var notification = NotificationType.Error;
                var message = _messageService.GetResourceError("UserIdNotFound");
                return RedirectToAction("RedirectToIndex", new { notification, message });
            }


            // Obtener todos los roles disponibles y mapearlos al modelo.
            var roles = await _rolesService.GetAllAsync();
            var listaRoles = roles
                .Select(role => new UsersRolesViewModel
                {
                    RolId = role.Id,
                    Name = role.Name,
                    IsSelected = userInfo.AspNetUserRoles.Any(userRole => userRole.RoleId == role.Id)
                })
                .OrderBy(role => role.Name)
                .ToList();

            // Crear el modelo para la vista.
            var usersViewModel = new UsersListViewModel
            {
                Id = userInfo.Id,
                Name = userInfo.UserName,
                listAccess = listaRoles
            };

            // Retornar la vista con el modelo.
            return View(usersViewModel);

        }

        /// <summary>
        /// Metodo que obtiene datos por Post y modifica los perfiles de los usuarios
        /// </summary>
        /// <param name="usersListViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Update(UsersListViewModel usersListViewModel)
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
                    var users = new AspNetUser()
                    {
                        Id = usersListViewModel.Id,
                        UserName = usersListViewModel.UserName,
                    };

                    var listPermissionsMenu = (from access in usersListViewModel.listAccess
                                               where access.IsSelected
                                               select new AspNetUserRole()
                                               {
                                                   UserId = users.Id,
                                                   RoleId = access.RolId,
                                               }
                                               ).ToList();

                    if (await _userService.UpdateAsync(users, listPermissionsMenu))
                    {
                        //se busca el usuario que se modifico para acceder a su username y mandarlo en el mensaje
                        var user = await _userService.FindAsync(usersListViewModel.Id);

                        notification = NotificationType.Success;
                        message = string.Format(_messageService.GetResourceMessage("UserSuccessfullyUpdated"), user.UserName);

                        #region Se registra el Log del evento (Actualizacion de los roles del usuario)
                        //se registra el evento de la actualizacion de los roles del usuario en la tabla de eventos
                        _logService.ActivityLog(_identityService.GetCurrentUserId(), "Actualización de roles al Usuario", $"Se actualizaron los roles del Usuario: {user.UserName}");
                        #endregion
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
                    _logService.ErrorLog($"Controller: Users, Action: {nameof(Update)}", ex);
                }
            }
            return RedirectToAction("Index", new { notification = notification, message = message });
        }

        public async Task<IActionResult> Delete([FromBody] string id)
        {           
            try
            {
                // Recuperar el usuario logueado
                var currentUser = _identityService.GetCurrentUserAsync();

                // Validar permisos del usuario.
                if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
                {
                    return RedirectToAction("Index", "Home");
                }

                // Intentar eliminar el usuario
                var deleteSuccess = await _userService.DeleteAsync(id);

                // Retornar el resultado como contenido textual
                return Content(deleteSuccess ? "true" : "false");
            }
            catch (Exception ex)
            {
                _logService.ErrorLog($"Controller: Users, Action: {nameof(Delete)}", ex);

                return Content("false");
            }
        }


    }
}
