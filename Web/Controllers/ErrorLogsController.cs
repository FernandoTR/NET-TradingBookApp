using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Enums;
using Web.Helpers;
using Web.Models;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class ErrorLogsController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IErrorLogService _errorLogService;

    private static int permissionNumber = (int)Permissions.Logs;

    public ErrorLogsController(IIdentityService identityService,
                               ILogService logService,
                               IMessageService messageService,
                               IErrorLogService errorLogService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _errorLogService = errorLogService;
    }

    #region Carga de datos en el DataTable

    public string draw = string.Empty;
    public string start = string.Empty;
    public string length = string.Empty;
    public string sortColumn = string.Empty;
    public string sortColumnDir = string.Empty;
    public string searchValue = string.Empty;

    public int pageSize;
    public int skip;
    public int recordsTotal;

    [HttpPost]
    public async Task<ActionResult> JsonDataTable(string fecha1, string fecha2)
    {
        if (!HasLogsPermission())
            return Forbid();

        DateTime dateStart = DateTime.Now.AddDays(-7);
        DateTime dateEnd = DateTime.Now;

        if (!string.IsNullOrEmpty(fecha1) && !string.IsNullOrEmpty(fecha2))
        {
            fecha1 += " 00:00:00.000";
            fecha2 += " 23:59:59.000";

            dateStart = DateTime.Parse(fecha1);
            dateEnd = DateTime.Parse(fecha2);
        }

        var data = new List<ErrorLogsViewModel>();

        try
        {
            var form = await Request.ReadFormAsync();

            draw = form["draw"].FirstOrDefault() ?? string.Empty;
            start = form["start"].FirstOrDefault() ?? string.Empty;
            length = form["length"].FirstOrDefault() ?? string.Empty;
            sortColumn = form[$"columns[{form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault() ?? string.Empty;
            sortColumnDir = form["order[0][dir]"].FirstOrDefault() ?? string.Empty;
            searchValue = form["search[value]"].FirstOrDefault() ?? string.Empty;

            pageSize = length != null ? Convert.ToInt32(length) : 0;
            skip = start != null ? Convert.ToInt32(start) : 0;
            recordsTotal = 0;

            var query = (await _errorLogService
                    .GetAllByDateRangeAsync(dateStart, dateEnd))
                .Select(x => new ErrorLogsViewModel
                {
                    Id = x.Id,
                    LogDate = x.LogDate,
                    MethodName = x.MethodName,
                    ExceptionMessage = x.ExceptionMessage,
                    ApplicationId = x.ApplicationId,
                    Task = ActionButtonHelper.GenerateActionMenu(new ActionMenuModel
                    {
                        Id = x.Id.ToString(),
                        ActionOptionMenus = new List<ActionOptionMenuModel>
                        {
                            new ActionOptionMenuModel
                            {
                                ActionType = ActionType.View,
                                JavaScriptAction = "showErrorLogDetail",
                            },
                        }
                    })
                });

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => (x.Id + x.LogDate.ToString() + x.MethodName + x.ExceptionMessage + x.ApplicationId)
                    .Contains(searchValue));
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = sortColumn switch
                {
                    "id" => sortColumnDir == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                    "logDate" => sortColumnDir == "asc" ? query.OrderBy(x => x.LogDate) : query.OrderByDescending(x => x.LogDate),
                    "methodName" => sortColumnDir == "asc" ? query.OrderBy(x => x.MethodName) : query.OrderByDescending(x => x.MethodName),
                    "exceptionMessage" => sortColumnDir == "asc" ? query.OrderBy(x => x.ExceptionMessage) : query.OrderByDescending(x => x.ExceptionMessage),
                    "applicationId" => sortColumnDir == "asc" ? query.OrderBy(x => x.ApplicationId) : query.OrderByDescending(x => x.ApplicationId),
                    _ => query
                };
            }

            recordsTotal = query.Count();
            data = query.Skip(skip).Take(pageSize).ToList();
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: ErrorLogs, Action: JsonDataTable", ex);
        }

        return Json(new
        {
            draw = draw,
            recordsFiltered = recordsTotal,
            recordsTotal = recordsTotal,
            data = data
        });
    }

    #endregion

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        if (!HasLogsPermission())
            return Forbid();

        try
        {
            var errorLog = await _errorLogService.GetByIdAsync(id);

            if (errorLog == null)
                return NotFound();

            return Json(new ErrorLogsViewModel
            {
                Id = errorLog.Id,
                LogDate = errorLog.LogDate,
                MethodName = errorLog.MethodName,
                ExceptionMessage = errorLog.ExceptionMessage,
                ExceptionStackTrace = errorLog.ExceptionStackTrace,
                ExceptionString = errorLog.ExceptionString,
                ApplicationId = errorLog.ApplicationId,
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: ErrorLogs, Action: {nameof(Detail)}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public IActionResult Index(NotificationType? notification, string message)
    {
        try
        {
            var currentUser = _identityService.GetCurrentUserAsync();

            if (User.Identity?.IsAuthenticated != true || currentUser == null || !currentUser.IsValid)
            {
                return currentUser?.ResetFlag == true
                    ? RedirectToAction("ChangePassword", "Manage")
                    : RedirectToAction("SignIn", "Account");
            }

            if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrEmpty(message) && notification != null)
            {
                ViewData[$"notifications.{notification}"] = message;
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: ErrorLogs, Action: {nameof(Index)}", ex);
            return RedirectToAction("index", "Home");
        }

        return View();
    }

    private bool HasLogsPermission()
    {
        var currentUser = _identityService.GetCurrentUserAsync();

        return User.Identity?.IsAuthenticated == true
            && currentUser != null
            && currentUser.IsValid
            && currentUser.PermissionNumberList != null
            && currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber));
    }
}
