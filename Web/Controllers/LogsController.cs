using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class LogsController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IActivityLogService _activityLogService;

    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.Logs;

    public LogsController(IIdentityService identityService,
                          ILogService logService,
                          IMessageService messageService,
                          IActivityLogService activityLogService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _activityLogService = activityLogService;
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

        var data = new List<LogsViewModel>();

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
            var query = (await _activityLogService
                        .GetAllLogsByDateRangeAsync(dateStart, dateEnd))
                        .Select(x => new LogsViewModel
                        {
                            EventId = x.Id,
                            EventDate = x.LogDate,
                            EventType = x.EventType,
                            Description = x.Description,
                            UserId = x.UserId,
                            UserName = x.User.UserName,
                        });

            // Filtrado por búsqueda (si existe)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => (x.EventId + x.EventDate.ToString() + x.EventType + x.Description + x.UserName)
                                          .Contains(searchValue));
            }

            // Ordenación
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = sortColumn switch
                {
                    "eventId" => sortColumnDir == "asc" ? query.OrderBy(x => x.EventId) : query.OrderByDescending(x => x.EventId),
                    "eventDate" => sortColumnDir == "asc" ? query.OrderBy(x => x.EventDate) : query.OrderByDescending(x => x.EventDate),
                    "eventType" => sortColumnDir == "asc" ? query.OrderBy(x => x.EventType) : query.OrderByDescending(x => x.EventType),
                    "description" => sortColumnDir == "asc" ? query.OrderBy(x => x.Description) : query.OrderByDescending(x => x.Description),
                    "userName" => sortColumnDir == "asc" ? query.OrderBy(x => x.UserName) : query.OrderByDescending(x => x.UserName),
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
            _logService.ErrorLog($"Controller: Logs, Action: JsonDataTable", ex);
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
}
