using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class AnalyticsConvergenceController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly ICatConvergenceService _catConvergenceService;
    private readonly ICatCategoryService _catCategoryService;
    private readonly ICatAccountTypeService _catAccountTypeService;
    private readonly ICatInstrumentsService _catInstrumentsService;
    private readonly ICatTriggerService _catTriggerService;
    private readonly ICatSceneryService _catSceneryService;
    private readonly ICatDirectionService _catDirectionService;
    private readonly ICatFrameService _catFrameService;
    private readonly ICatFigureService _catFigureService;

    private static int permissionNumber = (int)Permissions.AnalyticsConvergence;

    public AnalyticsConvergenceController(IIdentityService identityService,
                                    ILogService logService,
                                    IMessageService messageService,
                                    ICatConvergenceService catConvergenceService,
                                    ICatCategoryService catCategoryService,
                                    ICatAccountTypeService catAccountTypeService,
                                    ICatInstrumentsService catInstrumentsService,
                                    ICatTriggerService catTriggerService,
                                    ICatSceneryService catSceneryService,
                                    ICatDirectionService catDirectionService,
                                    ICatFrameService catFrameService,
                                    ICatFigureService catFigureService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _catConvergenceService = catConvergenceService;
        _catCategoryService = catCategoryService;
        _catAccountTypeService = catAccountTypeService;
        _catInstrumentsService = catInstrumentsService;
        _catTriggerService = catTriggerService;
        _catSceneryService = catSceneryService;
        _catDirectionService = catDirectionService;
        _catFrameService = catFrameService;
        _catFigureService = catFigureService;
    }

    #region Carga de datos en el DataTable

    [HttpPost]
    public async Task<ActionResult> JsonDataTable()
    {
        var data = new List<GetTBAnalyticsConvergenceDto>();
        var draw = string.Empty;
        var recordsTotal = 0;

        try
        {
            var form = await Request.ReadFormAsync();

            draw = form["draw"].FirstOrDefault();
            var start = form["start"].FirstOrDefault();
            var length = form["length"].FirstOrDefault();
            var sortColumn = form[$"columns[{form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
            var sortColumnDir = form["order[0][dir]"].FirstOrDefault();
            var searchValue = form["search[value]"].FirstOrDefault();

            var categoryId = form["categoryId"].FirstOrDefault();
            var accountTypeId = form["accountTypeId"].FirstOrDefault();
            var instrumentId = form["instrumentId"].FirstOrDefault();
            var triggerId = form["triggerId"].FirstOrDefault();
            var sceneryId = form["sceneryId"].FirstOrDefault();
            var directionId = form["directionId"].FirstOrDefault();
            var frameId = form["frameId"].FirstOrDefault();
            var figureId = form["figureId"].FirstOrDefault();
            var triggerActive = form["triggerActive"].FirstOrDefault();
            var sceneryActive = form["sceneryActive"].FirstOrDefault();
            var directionActive = form["directionActive"].FirstOrDefault();
            var frameActive = form["frameActive"].FirstOrDefault();
            var figureActive = form["figureActive"].FirstOrDefault();
            var minTrades = form["minTrades"].FirstOrDefault();

            var pageSize = !string.IsNullOrEmpty(length) ? Convert.ToInt32(length) : 10;
            var skip = !string.IsNullOrEmpty(start) ? Convert.ToInt32(start) : 0;

            var parameters = new ParametersTBAnalyticsConvergenceDto
            {
                CategoryId = !string.IsNullOrEmpty(categoryId) ? Convert.ToInt32(categoryId) : null,
                AccountTypeId = !string.IsNullOrEmpty(accountTypeId) ? Convert.ToInt32(accountTypeId) : null,
                InstrumentId = !string.IsNullOrEmpty(instrumentId) ? Convert.ToInt32(instrumentId) : null,
                TriggerId = !string.IsNullOrEmpty(triggerId) ? Convert.ToInt32(triggerId) : null,
                SceneryId = !string.IsNullOrEmpty(sceneryId) ? Convert.ToInt32(sceneryId) : null,
                DirectionId = !string.IsNullOrEmpty(directionId) ? Convert.ToInt32(directionId) : null,
                FrameId = !string.IsNullOrEmpty(frameId) ? Convert.ToInt32(frameId) : null,
                FigureId = !string.IsNullOrEmpty(figureId) ? Convert.ToInt32(figureId) : null,
                TriggerActive = !string.IsNullOrEmpty(triggerActive) && Convert.ToBoolean(triggerActive),
                SceneryActive = !string.IsNullOrEmpty(sceneryActive) && Convert.ToBoolean(sceneryActive),
                DirectionActive = !string.IsNullOrEmpty(directionActive) && Convert.ToBoolean(directionActive),
                FrameActive = !string.IsNullOrEmpty(frameActive) && Convert.ToBoolean(frameActive),
                FigureActive = !string.IsNullOrEmpty(figureActive) && Convert.ToBoolean(figureActive),
                MinTrades = !string.IsNullOrEmpty(minTrades) ? Convert.ToInt32(minTrades) : 10,
                SearchValue = searchValue,
                OrderByColumn = sortColumn,
                SortColumnDir = sortColumnDir,
                Skip = skip,
                Take = pageSize
            };

            var result = await _catConvergenceService.GetTBAnalyticsConvergenceAsync(parameters);
            data = result.data;
            recordsTotal = result.totalCount;
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: AnalyticsConvergence, Action: JsonDataTable", ex);
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

    #region Métodos para obtener listados para los listBox

    public async Task<List<SelectListItem>> GetCategoryListSelect(int? selectedId)
    {
        var data = await _catCategoryService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetAccountTypeListSelect(int? selectedId)
    {
        var data = await _catAccountTypeService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = x.Description,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetInstrumentsListSelect(int? selectedId)
    {
        var data = await _catInstrumentsService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" }
        };

        selectItems.AddRange(data.OrderBy(o => o.Ticker).Select(x => new SelectListItem
        {
            Text = x.Ticker,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetTriggerListSelect(int? selectedId)
    {
        var data = await _catTriggerService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "Todos", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = $"{x.Code}",
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetSceneryListSelect(int? selectedId)
    {
        var data = await _catSceneryService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "Todos", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = $"{x.Code}",
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetDirectionListSelect(int? selectedId)
    {
        var data = await _catDirectionService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "Todos", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = $"{x.Code}",
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetFrameListSelect(int? selectedId)
    {
        var data = await _catFrameService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "Todos", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = $"{x.Code}",
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetFigureListSelect(int? selectedId)
    {
        var data = await _catFigureService.GetAllAsync();

        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" }
            };
        }

        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "Todos", Value = "" }
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = $"{x.Code}",
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id
        }));

        return selectItems;
    }
    #endregion

    public async Task<IActionResult> Index(NotificationType? notification, string message)
    {
        try
        {
            var currentUser = _identityService.GetCurrentUserAsync();

            if (!User.Identity.IsAuthenticated || currentUser == null || !currentUser.IsValid)
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
            _logService.ErrorLog($"Controller: AnalyticsConvergence, Action: {nameof(Index)}", ex);
            return RedirectToAction("index", "Home");
        }

        ViewBag.CategoryItems = await GetCategoryListSelect(1);
        ViewBag.AccountTypeItems = await GetAccountTypeListSelect(null);
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(null);
        ViewBag.TriggerItems = await GetTriggerListSelect(null);
        ViewBag.SceneryItems = await GetSceneryListSelect(null);
        ViewBag.DirectionItems = await GetDirectionListSelect(null);
        ViewBag.FrameItems = await GetFrameListSelect(null);
        ViewBag.FigureItems = await GetFigureListSelect(null);

        return View();
    }
}
