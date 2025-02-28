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
public class AnalyticsFigureController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly ICatFigureService _catFigureService;
    private readonly ICatCategoryService _catCategoryService;
    private readonly ICatAccountTypeService _catAccountTypeService;
    private readonly ICatInstrumentsService _catInstrumentsService;
    private readonly ICatFrameService _catFrameService;
    private readonly ICatDirectionService _catDirectionService;

    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.AnalyticsFigure;

    public AnalyticsFigureController(IIdentityService identityService,
                                    ILogService logService,
                                    IMessageService messageService,
                                    ICatFigureService catFigureService,
                                    ICatCategoryService catCategoryService,
                                    ICatAccountTypeService catAccountTypeService,
                                    ICatInstrumentsService catInstrumentsService,
                                    ICatFrameService catFrameService,
                                    ICatDirectionService catDirectionService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _catFigureService = catFigureService;
        _catCategoryService = catCategoryService;
        _catAccountTypeService = catAccountTypeService;
        _catInstrumentsService = catInstrumentsService;
        _catFrameService = catFrameService;
        _catDirectionService = catDirectionService;
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
    public async Task<ActionResult> JsonDataTable(int categoryId, int accountTypeId, int instrumentId, int frameId, int directionId)
    {
        var data = new List<GetTBAnalyticsFigureDto>();

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

            var query = await _catFigureService.GetTBAnalyticsFigureAsync(new ParametersTBAnalyticsDto
            {
                CategoryId = categoryId,
                AccountTypeId = accountTypeId,
                InstrumentId = instrumentId,
                FrameId = frameId,
                DirectionId = directionId,
                SearchValue = searchValue,
                OrderByColumn = "Id",
                SortColumnDir = "ASC",
                Skip = skip,
                Take = pageSize
            });

            // Obtener el total de registros
            recordsTotal = query.Count();

            // Paginación: saltar los registros ya mostrados y tomar el número de registros indicado
            data = query.Skip(skip).Take(pageSize).ToList();


        }
        catch (Exception ex)
        {
            // Manejo de errores y registro de la excepción
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: AnalyticsFigure, Action: JsonDataTable", ex);
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

    /// <summary>
    /// Obtiene una lista de direciones en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID de la direccion que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con las direcciones disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetDirectionListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catDirectionService.GetAllAsync();

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


        ViewBag.CategoryItems = await GetCategoryListSelect(1);
        ViewBag.AccountTypeItems = await GetAccountTypeListSelect(null);
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(null);
        ViewBag.FrameItems = await GetFrameListSelect(null);
        ViewBag.DirectionItems = await GetDirectionListSelect(null);

        return View();
    }
}
