using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Email.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Helpers;
using Web.Models;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IOrdersService _ordersService;
    private readonly ICatCategoryService _catCategoryService;
    private readonly ICatAccountTypeService _catAccountTypeService;
    private readonly ICatInstrumentsService _catInstrumentsService;
    private readonly ICatFrameService _catFrameService;
    private readonly ICatDayService _catDayService;
    private readonly ICatStageService _catStageService;
    private readonly ICatFigureService _catFigureService;
    private readonly ICatTriggerService _catTriggerService;
    private readonly ICatDirectionService _catDirectionService;
    private readonly ICatSceneryService _catSceneryService;
    private readonly IAccountsService _accountsService;
  
    // Identificador del permiso 
    private static int permissionNumber = (int)Permissions.Orders;

    public OrdersController(IIdentityService identityService,
                                    ILogService logService,
                                    IMessageService messageService,
                                    IOrdersService ordersService,
                                    ICatCategoryService catCategoryService,
                                    ICatAccountTypeService catAccountTypeService,
                                    ICatInstrumentsService catInstrumentsService,
                                    ICatFrameService catFrameService,
                                    ICatDayService catDayService,
                                    ICatStageService catStageService,
                                    ICatFigureService catFigureService,
                                    ICatTriggerService catTriggerService,
                                    ICatDirectionService catDirectionService,
                                    ICatSceneryService catSceneryService,
                                    IAccountsService accountsService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _ordersService = ordersService;
        _catCategoryService = catCategoryService;
        _catAccountTypeService = catAccountTypeService;
        _catInstrumentsService = catInstrumentsService;
        _catFrameService = catFrameService;
        _catDayService = catDayService;
        _catStageService = catStageService;
        _catFigureService = catFigureService;
        _catTriggerService = catTriggerService;
        _catDirectionService = catDirectionService;
        _catSceneryService = catSceneryService;
        _accountsService = accountsService;
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
        var data = new List<GetOrdersDataTableDto>();

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

            var result = await _ordersService.GetOrdersDataTableAsync(new ParametersTBAnalyticsDto
                        {
                            CategoryId = categoryId,
                            AccountTypeId = accountTypeId,
                            InstrumentId = instrumentId,
                            FrameId = frameId,
                            DirectionId = directionId,
                            SearchValue = searchValue,
                            OrderByColumn = "Id",
                            SortColumnDir = "DESC",
                            Skip = skip,
                            Take = pageSize
                        });

            data = result.Item1.Select(x => new GetOrdersDataTableDto
                        {
                            Id = x.Id,
                            StatusId = x.StatusId,
                            CategoryName = x.CategoryName,
                            AccountTypeName = x.AccountTypeName,
                            SymbolName = x.SymbolName,
                            CreationDate = (DateTime)x.CreationDate,
                            TimeFormat = x.TimeFormat,
                            DayName = x.DayName,
                            StageName = x.StageName,
                            FigureName = x.FigureName,
                            FrameName = x.FrameName,
                            TriggerName = x.TriggerName,
                            DirectionName = x.DirectionName,
                            SceneryName = x.SceneryName,
                            StatusStyleName = x.StatusStyleName,
                            SLStyle = x.SLStyle,
                            TP1Style = x.TP1Style,
                            TP2Style = x.TP2Style,
                            TP3Style = x.TP3Style,
                            Target = x.Target,
                            Chart = x.Chart,
                            Task = (x.StatusId != 1) ? "" : ActionButtonHelper.GenerateActionMenu(new ActionMenuModel
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
                                     JavaScriptAction = "toDeleteOrder",
                                 },
                                 new ActionOptionMenuModel
                                 {
                                     Title = "Cerrar",
                                     JavaScriptAction = "showModalForClose",
                                 },
                                }
                            })
                        }).ToList();
           
            // Obtener el total de registros
            recordsTotal = result.count;

        }
        catch (Exception ex)
        {
            // Manejo de errores y registro de la excepción
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: Orders, Action: JsonDataTable", ex);
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
    /// Obtiene una lista de frames en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID del dia que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los días disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetDayListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catDayService.GetAllAsync();

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
    /// Obtiene una lista de etapas en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID de la etapa que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con las etapas disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetStageListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catStageService.GetAllAsync();

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
    /// Obtiene una lista de figuras en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID de la figura que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con las figuras disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetFigureListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catFigureService.GetAllAsync();

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
    /// Obtiene una lista de gatillos en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID de los gatillos que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los gatillos disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetTriggerListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catTriggerService.GetAllAsync();

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

    /// <summary>
    /// Obtiene una lista de escenarios en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <param name="selectedId">ID del escenarios que debe aparecer seleccionada en la lista, si aplica. Puede ser null.</param>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los escenarios disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetSceneryListSelect(int? selectedId)
    {
        // Obtiene todas las categorías disponibles
        var data = await _catSceneryService.GetAllAsync();

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
    /// Obtiene una lista de tipos de ordenes en formato <see cref="SelectListItem"/> para cargar controles como ListBox o DropDownList.
    /// </summary>
    /// <returns>Una lista de objetos <see cref="SelectListItem"/> con los tipos de ordenes disponibles.</returns>
    /// <remarks>
    /// El primer elemento de la lista es un valor vacío que puede usarse como placeholder.
    /// </remarks>
    public async Task<List<SelectListItem>> GetOrderTypeListSelect()
    {     
        // Construye la lista de categorías
        var selectItems = new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" }, // Placeholder inicial
            new SelectListItem { Text = "Market", Value = "Market" },
            new SelectListItem { Text = "Limit", Value = "Limit", Selected = true},
            new SelectListItem { Text = "Stop", Value = "Stop" },
        };

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

    public async Task<IActionResult> New()
    {
        ViewBag.CategoryItems = await GetCategoryListSelect(1);
        ViewBag.AccountTypeItems = await GetAccountTypeListSelect(null);
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(null);
        ViewBag.DayItems = await GetDayListSelect(null);
        ViewBag.StageItems = await GetStageListSelect(null);
        ViewBag.FigureItems = await GetFigureListSelect(null);
        ViewBag.FrameItems = await GetFrameListSelect(null);
        ViewBag.TriggerItems = await GetTriggerListSelect(null);
        ViewBag.DirectionItems = await GetDirectionListSelect(null);
        ViewBag.SceneryItems = await GetSceneryListSelect(null);
        ViewBag.OrderTypeItems = await GetOrderTypeListSelect();

        return View();
    }

    /// <summary>
    /// Guarda una nueva orden.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddOrder([FromBody] OrdersCreateViewModel model)
    {
        try
        {
            var accounts = (await _accountsService.GetAllAsync())
                .Where(o => o.UserId == _identityService.GetCurrentUserId() && o.CatAccountTypeId == model.AccountTypeId).FirstOrDefault();

            if (accounts.CurrentBalance < Convert.ToDecimal(model.Total))
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "La cuenta seleccionada no tiene saldo suficiente para procesar la orden.",
                    notificationType = NotificationType.Error
                });
            }



            var order = new Order
            {
                AlterDate = DateTime.Now,
                AuthorId = _identityService.GetCurrentUserId(),
                CatCategoryId = model.CategoryId,
                AccountId = accounts.Id,
                CatInstrumentId = model.InstrumentsId,
                Date = DateOnly.FromDateTime(model.CreationDate),
                Time = TimeOnly.FromTimeSpan(model.Time),
                CatDayId = model.DayId,
                CatStageId = model.StageId,
                CatFigureId = model.FigureId,
                CatFrameId = model.FrameId,
                CatTriggerId = model.TriggerId,
                CatDirectionId = model.DirectionId,
                CatSceneryId = model.SceneryId,
                CatStatusId = 1,

                // Contexto estructural
                IsTrendAligned = model.IsTrendAligned,
                LocationType = model.LocationType,
                ConfirmationType = model.ConfirmationType,
                IsPivotZone = model.IsPivotZone,

                Sl = false,
                Tp1 = false,
                Tp2 = false,
                Tp3 = false,
                Target = 0m,
                Chart = "",
            };

            var trade = new Trade
            {
                OrderType = model.OrderTypeId,
                TradeType = model.TradeTypeId,
                Quantity = Convert.ToDecimal(model.Quantity),
                Price = Convert.ToDecimal(model.Price),
                CommissionRate = Convert.ToDecimal(model.CommissionRate),
                Total = Convert.ToDecimal(model.Total),
                TradeDate = DateTime.Now,
            };

            var result = await _ordersService.AddOrderAsync(order, trade);

            if (!result.Item1)
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "La nueva orden no se procesó correctamente.",
                    notificationType = NotificationType.Error
                });
            }

            var accountBalance = new AccountBalance
            {
                AccountId = accounts.Id,
                OrderId = result.Item2,
                Balance = Convert.ToDecimal(model.Total),
                Reference = $"Tipo: {model.OrderTypeId}, Acción: {model.TradeTypeId}"
            };


            // Descuenta el monto de la cuenta seleccionada
            await _accountsService.WithDrawCashAsync(accountBalance);

            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = string.Format(_messageService.GetResourceMessage("OrderSuccessfullySaved"), result.Item2),
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Orders, Action: {nameof(AddOrder)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
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

        OrdersViewModel model = null;

        try
        {
            var order = await _ordersService.GetByIdAsync(id);

            var accounts = await _accountsService.GetByIdAsync(order.AccountId);

            TimeOnly timeOnlyValue = order.Time;
            DateOnly fecha = DateOnly.FromDateTime(DateTime.Now);

            model = new OrdersViewModel()
            {
                Id = order.Id,
                AlterDate = order.AlterDate,
                AuthorId = order.AuthorId,
                CategoryId = order.CatCategoryId,
                AccountTypeId = accounts.CatAccountTypeId,
                InstrumentsId = order.CatInstrumentId,
                CreationDate = fecha.ToDateTime(TimeOnly.MinValue),
                Time = timeOnlyValue.ToTimeSpan(),
                DayId = order.CatDayId,
                StageId = order.CatStageId,
                FigureId = order.CatFigureId,
                FrameId = order.CatFrameId,
                TriggerId = order.CatTriggerId,
                DirectionId = order.CatDirectionId,
                SceneryId = order.CatSceneryId,
                StatusId = order.CatStatusId,
                IsTrendAligned = order.IsTrendAligned,
                LocationType = order.LocationType,
                ConfirmationType = order.ConfirmationType,
                IsPivotZone = order.IsPivotZone,
                Grade = order.Grade,
                TotalScore = order.TotalScore,
            };

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
            _logService.ErrorLog($"Controller: Orders, Action: {nameof(Edit)}", ex);
        }

        ViewBag.CategoryItems = await GetCategoryListSelect(model.CategoryId);
        ViewBag.AccountTypeItems = await GetAccountTypeListSelect(model.AccountTypeId);
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(model.InstrumentsId);
        ViewBag.DayItems = await GetDayListSelect(model.DayId);
        ViewBag.StageItems = await GetStageListSelect(model.StatusId);
        ViewBag.FigureItems = await GetFigureListSelect(model.FigureId);
        ViewBag.FrameItems = await GetFrameListSelect(model.FrameId);
        ViewBag.TriggerItems = await GetTriggerListSelect(model.TriggerId);
        ViewBag.DirectionItems = await GetDirectionListSelect(model.DirectionId);
        ViewBag.SceneryItems = await GetSceneryListSelect(model.SceneryId);
        ViewBag.OrderTypeItems = await GetOrderTypeListSelect();

        return View(model);
    }

    /// <summary>
    /// Actualiza una orden existente en base al modelo proporcionado.
    /// </summary>
    /// <param name="model">Modelo con los datos de la orden a actualizar.</param>
    /// <returns>Redirección a la acción "Index" con una notificación y mensaje.</returns>
    [HttpPost]
    public async Task<IActionResult> Update(OrdersViewModel model)
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
            var accounts = (await _accountsService.GetAllAsync())
               .Where(o => o.UserId == _identityService.GetCurrentUserId() && o.CatAccountTypeId == model.AccountTypeId).FirstOrDefault();

            var order = new Order()
            {
                Id = model.Id,
                AlterDate = DateTime.Now,
                AuthorId = _identityService.GetCurrentUserId(),
                CatCategoryId = model.CategoryId,
                AccountId = accounts.Id,
                CatInstrumentId = model.InstrumentsId,
                Date = DateOnly.FromDateTime(model.CreationDate),
                Time = TimeOnly.FromTimeSpan(model.Time),
                CatDayId = model.DayId,
                CatStageId = model.StageId,
                CatFigureId = model.FigureId,
                CatFrameId = model.FrameId,
                CatTriggerId = model.TriggerId,
                CatDirectionId = model.DirectionId,
                CatSceneryId = model.SceneryId,
                CatStatusId = 1,

                // Contexto estructural
                IsTrendAligned = model.IsTrendAligned,
                LocationType = model.LocationType,
                ConfirmationType = model.ConfirmationType,
                IsPivotZone = model.IsPivotZone,

                Sl = false,
                Tp1 = false,
                Tp2 = false,
                Tp3 = false,
                Target = 0m,
                Chart = "",
            };

            // Intentar actualizar la orden
            if (await _ordersService.UpdateAsync(order))
            {
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Success,
                    message = string.Format(_messageService.GetResourceMessage("OrderSuccessfullyUpdated"), model.Id)
                });                           
            }

            // Si la actualización falla
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = _messageService.GetResourceError("GenericError")
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Orders, Action: {nameof(Update)}", ex);

            // Redirigir con notificación de error
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = _messageService.GetResourceError("GenericError")
            });
        }
       
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
            if (await _ordersService.DeleteOrderAsync(id))
            {
                return Content("true");
            }

            return Content("false");
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Employees, Action: {nameof(Delete)}", ex);
            return Content("false");
        }
        
    }


    public async Task<IActionResult> Close(int id)
    {
        // Recuperar el usuario logueado
        var currentUser = _identityService.GetCurrentUserAsync();

        // Validar permisos del usuario.
        if (currentUser.PermissionNumberList == null || !currentUser.PermissionNumberList.Any(x => x.Equals(permissionNumber)))
        {
            return RedirectToAction("Index", "Home");
        }

        OrdersSellViewModel model = null;

        try
        {
            var order = await _ordersService.GetByIdAsync(id);

            model = new OrdersSellViewModel()
            {
                Id = order.Id
            };

        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("FailedToFindItem");
            _logService.ErrorLog($"Controller: Orders, Action: {nameof(Edit)}", ex);
        }

        
        ViewBag.OrderTypeItems = await GetOrderTypeListSelect();

        return View(model);
    }

    /// <summary>
    /// Cierra una orden existente.
    /// </summary>
    /// <param name="model">Modelo que contiene los datos necesarios para cerrar la orden.</param>
    /// <returns>Devuelve un objeto JSON con el resultado de la operación.</returns>
    [HttpPost]
    public async Task<IActionResult> CloseOrder([FromBody] OrdersSellViewModel model)
    {
        try
        {
            // Obtiene la orden por su Id
            var order = await _ordersService.GetByIdAsync(model.Id);
            if (order == null) {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "La orden no se encontró.",
                    notificationType = NotificationType.Error
                });
            }

            // Actualiza los datos de la orden
            order.AlterDate = DateTime.Now;
            order.AuthorId = _identityService.GetCurrentUserId();
            order.CatStatusId = 2;
            order.Sl = model.SL;
            order.Tp1 = model.TP1;
            order.Tp2 = model.TP2;
            order.Tp3 = model.TP3;
            order.Target = model.Target;
            order.Chart = model.Chart;
            order.Comments = model.Comments;

            // Crea el registro del trade relacionado con la orden
            var trade = new Trade
            {
                OrderId = order.Id,
                OrderType = model.OrderTypeId,
                TradeType = model.TradeTypeId,
                Quantity = Convert.ToDecimal(model.Quantity),
                Price = Convert.ToDecimal(model.Price),
                CommissionRate = Convert.ToDecimal(model.CommissionRate),
                Total = Convert.ToDecimal(model.Total),
                TradeDate = DateTime.Now,
            };

            // Cerrar la orden
            if (!await _ordersService.CloseOrderAsync(order, trade))
            {
                return Json(new ResultBackViewModel
                {
                    Success = false,
                    Message = "La orden no se cerró correctamente.",
                    notificationType = NotificationType.Error
                });
            }

            // Actualiza el balance de la cuenta asociada
            var accountBalance = new AccountBalance
            {
                AccountId = order.AccountId,
                OrderId = order.Id,
                Balance = Convert.ToDecimal(model.Total),
                Reference = $"Tipo: {model.OrderTypeId}, Acción: {model.TradeTypeId}"
            };

            await _accountsService.AddCashAsync(accountBalance);


            // Devuelve una respuesta de éxito
            return Json(new ResultBackViewModel
            {
                Success = true,
                Message = string.Format(_messageService.GetResourceMessage("OrderSuccessfullyUpdated"), order.Id),
                notificationType = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Orders, Action: {nameof(CloseOrder)}", ex);
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = _messageService.GetResourceError("GenericError"),
                notificationType = NotificationType.Error
            });
        }
    }

}
