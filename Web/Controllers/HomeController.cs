using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Application.Services;
using Infrastructure;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing.Printing;
using Web.Models;
using Web.Models.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly ICatTriggerService _catTriggerService;
    private readonly ICatCategoryService _catCategoryService;
    private readonly ICatAccountTypeService _catAccountTypeService;
    private readonly ICatInstrumentsService _catInstrumentsService;
    private readonly ICatFrameService _catFrameService;
    private readonly IAccountsService _accountsService;
    private readonly IEmployeeService _employeeService;

    public HomeController(IIdentityService identityService,
                          ILogService logService,
                          ICatTriggerService catTriggerService,
                          ICatCategoryService catCategoryService,
                          ICatAccountTypeService catAccountTypeService,
                          ICatInstrumentsService catInstrumentsService,
                          ICatFrameService catFrameService,
                          IAccountsService accountsService,
                          IEmployeeService employeeService)
    {
        _identityService = identityService;
        _logService = logService;
        _catTriggerService = catTriggerService;
        _catCategoryService = catCategoryService;
        _catAccountTypeService = catAccountTypeService;
        _catInstrumentsService = catInstrumentsService;
        _catFrameService = catFrameService;
        _accountsService = accountsService;
        _employeeService = employeeService;
    }

    #region Métodos para obtener listados para los listBox
    /// <summary>
    /// Obtiene una lista de categorías en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID de la categoría que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con las categorías disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetCategoryListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catCategoryService.GetAllAsync();

        // Si no hay datos, retorna una lista con solo el placeholder
        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" } // Placeholder inicial
            };
        }

        // Construye la lista de categorías
        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" } // Placeholder inicial
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id // Marca como seleccionado si el ID coincide
        }));

        return selectItems;
    }

    /// <summary>
    /// Obtiene una lista de tipo de cuenta en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID del tipo que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los tipos de cuenta disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetAccountTypeListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catAccountTypeService.GetAllAsync();

        // Si no hay datos, retorna una lista con solo el placeholder
        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" } // Placeholder inicial
            };
        }

        // Construye la lista de categorías
        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" } // Placeholder inicial
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = x.Description,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id // Marca como seleccionado si el ID coincide
        }));

        return selectItems;
    }

    /// <summary>
    /// Obtiene una lista de instrumentos en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID del instrumento que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los instrumentos disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetInstrumentsListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catInstrumentsService.GetAllAsync();

        // Si no hay datos, retorna una lista con solo el placeholder
        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" } // Placeholder inicial
            };
        }

        // Construye la lista de categorías
        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" } // Placeholder inicial
        };

        selectItems.AddRange(data.OrderBy(o => o.Ticker).Select(x => new SelectListItem
        {
            Text = x.Ticker,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id // Marca como seleccionado si el ID coincide            
        }));

        return selectItems;
    }

    /// <summary>
    /// Obtiene una lista de frames en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID del frame que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los frames disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetFrameListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catFrameService.GetAllAsync();

        // Si no hay datos, retorna una lista con solo el placeholder
        if (data == null || !data.Any())
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = "" } // Placeholder inicial
            };
        }

        // Construye la lista de categorías
        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" } // Placeholder inicial
        };

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = x.Description,
            Value = x.Id.ToString(),
            Selected = selectedId == x.Id // Marca como seleccionado si el ID coincide
        }));

        return selectItems;
    }
    #endregion


    public async Task<IActionResult> Index(NotificationType? notification, string message)
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

            // Si hay un mensaje y una notificación, lo agregamos al ViewData
            if (!string.IsNullOrEmpty(message) && notification != null)
            {
                ViewData[$"notifications.{notification}"] = message;
            }

            ViewBag.FullName = $"{currentUser.Name} {currentUser.LastName}";
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Home, Action: {nameof(Index)}", ex);
            return RedirectToAction("Error", "Home");
        }

        ViewBag.CategoryItems = await GetCategoryListSelect(1);
        ViewBag.AccountTypeItems = await GetAccountTypeListSelect(null);
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(null);
        ViewBag.FrameItems = await GetFrameListSelect(null);

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        return PartialView("_Balance", model);
    }

    /// <summary>
    /// Muestra un resumen general de las estadisticas de gatillos.
    /// </summary>
    /// <returns>Vista parcial con los datos del resumen.</returns>
    [HttpPost]
    public async Task<PartialViewResult> AnalyticsTrigger([FromBody] ParametersAnalyticsDto parameters)
    {
        var query = await _catTriggerService.GetTBAnalyticsTriggerAsync(new ParametersTBAnalyticsDto
        {
            CategoryId = parameters.CategoryId,
            AccountTypeId = parameters.AccountTypeId,
            InstrumentId = parameters.InstrumentId,
            FrameId = parameters.FrameId,
            SearchValue = "",
            OrderByColumn = "Id",
            SortColumnDir = "ASC",
            Skip = 0,
            Take = 10
        });

        var model = new AnalyticsTriggerViewModel
        {
            AnalyticsTriggerList = query.ToList(),
            TotalRecords = query.Count(),
            TotalValidRecords = query.Where(x => x.TP1P >= 70).Count(),

        };

        return PartialView("_AnalyticsTrigger", model);
    }


    /// <summary>
    /// Obtiene la lista de gatillos para cargar en la grafica.
    /// </summary>
    /// <returns>Vista parcial con los datos del resumen.</returns>
    [HttpPost]
    public async Task<List<GetTBAnalyticsTriggerDto>> AnalyticsTriggerChart([FromBody] ParametersAnalyticsDto parameters)
    {
        var query = await _catTriggerService.GetTBAnalyticsTriggerAsync(new ParametersTBAnalyticsDto
        {
            CategoryId = parameters.CategoryId,
            AccountTypeId = parameters.AccountTypeId,
            InstrumentId = parameters.InstrumentId,
            FrameId = parameters.FrameId,
            SearchValue = "",
            OrderByColumn = "Id",
            SortColumnDir = "ASC",
            Skip = 0,
            Take = 10
        });

        return query.ToList();

    }

    /// <summary>
    /// Obtiene la lista de efectividad por bloque para cargar en la grafica.
    /// </summary>
    /// <returns>Vista parcial con los datos del resumen.</returns>
    [HttpPost]
    public async Task<IActionResult> AnalyticsLastBlocks([FromBody] ParametersAnalyticsDto parameters)
    {
        try
        {
            var query = await _catTriggerService.GetTBAnalyticsLastBlockAsync(parameters);

            return Json(new
            {
                result = true,
                List = query.ToList()
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Home, Action: {nameof(AnalyticsLastBlocks)}", ex);
            return Json(new
            {
                result = false
            });
        }
       
    }





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


}
