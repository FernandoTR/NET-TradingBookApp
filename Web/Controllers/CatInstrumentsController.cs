using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models.Enums;
using Web.Models;
using Infrastructure;

namespace Web.Controllers
{
    public class CatInstrumentsController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly ILogService _logService;
        private readonly IMessageService _messageService;
        private readonly ICatInstrumentsService _catInstrumentsService;

        // Identificador del permiso 
        private static int permissionNumber = (int)Permissions.CatInstruments;

        public CatInstrumentsController(IIdentityService identityService,
                                   ILogService logService,
                                   IMessageService messageService,
                                   ICatInstrumentsService catInstrumentsService)
        {
            _identityService = identityService;
            _logService = logService;
            _messageService = messageService;
            _catInstrumentsService = catInstrumentsService;
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
            var data = new List<CatInstrumentsViewModels>();

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
                var query = (await _catInstrumentsService
                            .GetAllAsync())
                            .Select(x => new CatInstrumentsViewModels
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Ticker = x.Ticker,
                                InstrumentType = x.InstrumentType,
                                Currency = x.Currency,
                                Market = x.Market,
                                IsActived = x.IsActived,
                                UpdatedAt = x.UpdatedAt,
                                LinkIcon = x.LinkIcon,
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
                    query = query.Where(x => (x.Id + x.Name + x.Ticker + x.InstrumentType + x.Currency + x.Market).Contains(searchValue));
                }

                // Ordenación
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    switch (sortColumn)
                    {
                        case "id":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                            break;
                        case "name":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                            break;
                        case "instrumentType":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.InstrumentType) : query.OrderByDescending(x => x.InstrumentType);
                            break;
                        case "currency":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Currency) : query.OrderByDescending(x => x.Currency);
                            break;
                        case "market":
                            query = sortColumnDir == "asc" ? query.OrderBy(x => x.Market) : query.OrderByDescending(x => x.Market);
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
                _logService.ErrorLog($"Controller: CatInstruments, Action: JsonDataTable", ex);
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
                _logService.ErrorLog($"Controller: CatInstruments, Action: {nameof(Index)}", ex);
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
        public async Task<IActionResult> Save(CatInstrumentsViewModels model)
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
                var instrument = new CatInstrument()
                {
                    Name = model.Name.Trim(),
                    Ticker = model.Ticker.Trim(),
                    InstrumentType = model.InstrumentType.Trim(),
                    Currency = model.Currency.Trim(),
                    Market = model.Market.Trim(),
                    IsActived = true,
                    UpdatedAt = DateTime.Now,
                    LinkIcon = model.LinkIcon.Trim()
                };

                if (await _catInstrumentsService.AddAsync(instrument))
                {
                    notification = NotificationType.Success;
                    message = string.Format(_messageService.GetResourceMessage("InstrumentsSuccessfullySaved"), model.Name);
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
                _logService.ErrorLog($"Controller: CatInstrumets, Action: {nameof(Save)}", ex);
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

            CatInstrumentsViewModels model = null;
            try
            {
                var instrument = await _catInstrumentsService.GetByIdAsync(id);
                model = new CatInstrumentsViewModels()
                {
                    Id = instrument.Id,
                    Name = instrument.Name,
                    Ticker = instrument.Ticker,
                    InstrumentType = instrument.InstrumentType,
                    Currency = instrument.Currency,
                    Market = instrument.Market,
                    IsActived = (bool)instrument.IsActived,
                    UpdatedAt = instrument.UpdatedAt,
                    LinkIcon = instrument.LinkIcon
                };

            }
            catch (Exception ex)
            {
                ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
                _logService.ErrorLog($"Controller: CatInstruments, Action: {nameof(Edit)}", ex);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CatInstrumentsViewModels model)
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
                var instrument = new CatInstrument()
                {
                    Id = model.Id,
                    Name = model.Name.Trim(),
                    Ticker = model.Ticker.Trim(),
                    InstrumentType = model.InstrumentType.Trim(),
                    Currency = model.Currency.Trim(),
                    Market = model.Market.Trim(),
                    IsActived = (bool)model.IsActived,
                    UpdatedAt = DateTime.Now,
                    LinkIcon = model.LinkIcon.Trim()
                };

                if (await _catInstrumentsService.UpdateAsync(instrument))
                {
                    notification = NotificationType.Success;
                    message = string.Format(_messageService.GetResourceMessage("InstrumentsSuccessfullyUpdated"), model.Name);
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
                _logService.ErrorLog($"Controller: CatInstruments, Action: {nameof(Update)}", ex);
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
                bool isDeleted = await _catInstrumentsService.DeleteAsync(id);

                // Devolver un resultado indicando si la eliminación fue exitosa
                return Content(isDeleted.ToString().ToLower());
            }
            catch (Exception ex)
            {
                _logService.ErrorLog($"Controller: CatInstruments, Action: {nameof(Delete)}", ex);
                return Content("false");
            }
        }

    }
}
