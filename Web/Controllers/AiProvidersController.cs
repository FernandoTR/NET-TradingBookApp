using Application.DTOs;
using Application.DTOs.AiProviders;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Enums;
using Web.Helpers;
using Web.Models;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class AiProvidersController : Controller
{
    private const string OpenAiProviderName = "OpenAI";
    private const string OpenCodeGoProviderName = "OpenCodeGo";

    private static readonly string[] SupportedProviders =
    [
        OpenAiProviderName,
        "MiniMax",
        "DeepSeek",
        "GLM",
        "Kimi",
        OpenCodeGoProviderName
    ];

    private static readonly string[] CatalogBackedProviders =
    [
        OpenAiProviderName,
        OpenCodeGoProviderName
    ];

    private static readonly string[] NewProviderOptions =
    [
        OpenAiProviderName,
        OpenCodeGoProviderName
    ];

    private static readonly int PermissionNumber = (int)Permissions.AiProviders;

    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IMessageService _messageService;
    private readonly IAiProviderConfigurationService _aiProviderConfigurationService;
    private readonly IAiProviderModelCatalogService _aiProviderModelCatalogService;

    public AiProvidersController(
        IIdentityService identityService,
        ILogService logService,
        IMessageService messageService,
        IAiProviderConfigurationService aiProviderConfigurationService,
        IAiProviderModelCatalogService aiProviderModelCatalogService)
    {
        _identityService = identityService;
        _logService = logService;
        _messageService = messageService;
        _aiProviderConfigurationService = aiProviderConfigurationService;
        _aiProviderModelCatalogService = aiProviderModelCatalogService;
    }

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
    public async Task<ActionResult> JsonDataTable(CancellationToken cancellationToken)
    {
        var data = new List<AiProviderConfigurationViewModel>();

        try
        {
            var form = await Request.ReadFormAsync();

            draw = form["draw"].FirstOrDefault();
            start = form["start"].FirstOrDefault();
            length = form["length"].FirstOrDefault();
            sortColumn = form[$"columns[{form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
            sortColumnDir = form["order[0][dir]"].FirstOrDefault();
            searchValue = form["search[value]"].FirstOrDefault();

            if (!HasPermission())
            {
                return Json(new
                {
                    draw,
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data
                });
            }

            pageSize = length != null ? Convert.ToInt32(length) : 10;
            skip = start != null ? Convert.ToInt32(start) : 0;

            var query = (await _aiProviderConfigurationService.GetAllAsync(cancellationToken))
                .Select(MapViewModel);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(provider => BuildSearchText(provider)
                    .Contains(searchValue.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = sortColumn switch
                {
                    "Id" => sortColumnDir == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id),
                    "ProviderName" => sortColumnDir == "asc" ? query.OrderBy(x => x.ProviderName) : query.OrderByDescending(x => x.ProviderName),
                    "ModelName" => sortColumnDir == "asc" ? query.OrderBy(x => x.ModelName) : query.OrderByDescending(x => x.ModelName),
                    "ApiKeyEnvironmentVariable" => sortColumnDir == "asc" ? query.OrderBy(x => x.ApiKeyEnvironmentVariable) : query.OrderByDescending(x => x.ApiKeyEnvironmentVariable),
                    "IsApiKeyConfigured" => sortColumnDir == "asc" ? query.OrderBy(x => x.IsApiKeyConfigured) : query.OrderByDescending(x => x.IsApiKeyConfigured),
                    "SupportsVision" => sortColumnDir == "asc" ? query.OrderBy(x => x.SupportsVision) : query.OrderByDescending(x => x.SupportsVision),
                    "IsActive" => sortColumnDir == "asc" ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
                    "IsEnabled" => sortColumnDir == "asc" ? query.OrderBy(x => x.IsEnabled) : query.OrderByDescending(x => x.IsEnabled),
                    _ => query
                };
            }

            recordsTotal = query.Count();
            data = pageSize == -1
                ? query.Skip(skip).ToList()
                : query.Skip(skip).Take(pageSize).ToList();
        }
        catch (Exception ex)
        {
            ViewData[$"notifications.{NotificationType.Error}"] = _messageService.GetResourceError("GenericError");
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(JsonDataTable)}", ex);
        }

        return Json(new
        {
            draw,
            recordsFiltered = recordsTotal,
            recordsTotal,
            data
        });
    }

    public IActionResult Index(NotificationType? notification, string message)
    {
        try
        {
            var unauthorized = EnsureAuthorized(out var currentUser);
            if (unauthorized is not null)
            {
                return unauthorized;
            }

            if (!string.IsNullOrEmpty(message) && notification != null)
            {
                ViewData[$"notifications.{notification}"] = message;
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(Index)}", ex);
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    public async Task<IActionResult> New(CancellationToken cancellationToken)
    {
        var unauthorized = EnsureAuthorized(out var currentUser);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new AiProviderConfigurationViewModel
        {
            SupportsVision = true,
            IsEnabled = true,
            TimeoutSeconds = 60
        };

        SetProviderItems(model.ProviderName, NewProviderOptions);
        await SetModelCatalogItemsAsync(model.ProviderName, model.ModelCatalogId, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var unauthorized = EnsureAuthorized(out var currentUser);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        try
        {
            var provider = await _aiProviderConfigurationService.GetByIdAsync(id, cancellationToken);
            if (provider is null)
            {
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Error,
                    message = _messageService.GetResourceError("FailedToFindItem")
                });
            }

            var model = MapViewModel(provider);
            SetProviderItems(model.ProviderName);
            await SetModelCatalogItemsAsync(model.ProviderName, model.ModelCatalogId, cancellationToken);
            return View(model);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(Edit)}", ex);
            return RedirectToAction("Index", new
            {
                notification = NotificationType.Error,
                message = _messageService.GetResourceError("GenericError")
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AiProviderConfigurationViewModel model, CancellationToken cancellationToken)
    {
        var unauthorized = EnsureAuthorized(out var currentUser);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        try
        {
            var saved = await _aiProviderConfigurationService.CreateAsync(ToDto(model), currentUser!.UserId, cancellationToken);
            if (saved)
            {
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Success,
                    message = $"Proveedor IA {model.ProviderName} guardado correctamente."
                });
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(Save)}", ex);
        }

        return RedirectToAction("Index", new
        {
            notification = NotificationType.Error,
            message = _messageService.GetResourceError("GenericError")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AiProviderConfigurationViewModel model, CancellationToken cancellationToken)
    {
        var unauthorized = EnsureAuthorized(out var currentUser);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        try
        {
            var updated = await _aiProviderConfigurationService.UpdateAsync(ToDto(model), currentUser!.UserId, cancellationToken);
            if (updated)
            {
                return RedirectToAction("Index", new
                {
                    notification = NotificationType.Success,
                    message = $"Proveedor IA {model.ProviderName} actualizado correctamente."
                });
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(Update)}", ex);
        }

        return RedirectToAction("Index", new
        {
            notification = NotificationType.Error,
            message = _messageService.GetResourceError("GenericError")
        });
    }

    [HttpPost]
    public async Task<IActionResult> Activate([FromBody] int id, CancellationToken cancellationToken)
    {
        if (!HasPermission())
        {
            return Content("false");
        }

        try
        {
            var result = await _aiProviderConfigurationService.ActivateAsync(id, _identityService.GetCurrentUserId(), cancellationToken);
            if (result)
            {
                return Content("true");
            }

            var provider = await _aiProviderConfigurationService.GetByIdAsync(id, cancellationToken);
            if (provider is not null &&
                IsCatalogBackedProvider(provider.ProviderName) &&
                !provider.SupportsVision)
            {
                return Content($"No se puede activar {provider.ProviderName} porque el modelo '{provider.ModelName}' no tiene soporte de vision confirmado.");
            }

            return Content("false");
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(Activate)}", ex);
            return Content("false");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate([FromBody] int id, CancellationToken cancellationToken)
    {
        if (!HasPermission())
        {
            return Content("false");
        }

        try
        {
            var result = await _aiProviderConfigurationService.DeactivateAsync(id, _identityService.GetCurrentUserId(), cancellationToken);
            return Content(result.ToString().ToLower());
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(Deactivate)}", ex);
            return Content("false");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetModelsByProvider(string providerName, CancellationToken cancellationToken)
    {
        if (!HasPermission())
        {
            return Json(Array.Empty<object>());
        }

        try
        {
            var models = await _aiProviderModelCatalogService.GetEnabledByProviderAsync(providerName, cancellationToken);
            return Json(models.Select(model => new
            {
                id = model.Id,
                providerName = model.ProviderName,
                modelName = model.ModelName,
                modelId = model.ModelId,
                endpoint = model.Endpoint,
                apiProtocol = model.ApiProtocol,
                supportsVision = model.SupportsVision
            }));
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: AiProviders, Action: {nameof(GetModelsByProvider)}", ex);
            return Json(Array.Empty<object>());
        }
    }

    private IActionResult? EnsureAuthorized(out CurrentUserDto? currentUser)
    {
        currentUser = _identityService.GetCurrentUserAsync();

        if (User.Identity?.IsAuthenticated != true || currentUser is null || !currentUser.IsValid)
        {
            return currentUser?.ResetFlag == true
                ? RedirectToAction("ChangePassword", "Manage")
                : RedirectToAction("SignIn", "Account");
        }

        if (!HasPermission(currentUser))
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private bool HasPermission()
    {
        var currentUser = _identityService.GetCurrentUserAsync();
        return currentUser is not null && HasPermission(currentUser);
    }

    private static bool HasPermission(CurrentUserDto currentUser)
    {
        return currentUser.PermissionNumberList is not null &&
            currentUser.PermissionNumberList.Any(permission => permission.Equals(PermissionNumber));
    }

    private static bool IsCatalogBackedProvider(string? providerName)
    {
        return CatalogBackedProviders.Any(provider =>
            string.Equals(provider, providerName?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private void SetProviderItems(string? selectedProvider, IEnumerable<string>? providerOptions = null)
    {
        ViewBag.ProviderItems = (providerOptions ?? SupportedProviders)
            .Select(provider => new SelectListItem
            {
                Text = provider,
                Value = provider,
                Selected = string.Equals(provider, selectedProvider, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();
    }

    private async Task SetModelCatalogItemsAsync(string? providerName, int? selectedModelCatalogId, CancellationToken cancellationToken)
    {
        if (!IsCatalogBackedProvider(providerName))
        {
            ViewBag.ModelCatalogItems = new List<SelectListItem>();
            return;
        }

        var models = await _aiProviderModelCatalogService.GetEnabledByProviderAsync(providerName, cancellationToken);
        ViewBag.ModelCatalogItems = models
            .Select(model => new SelectListItem
            {
                Text = model.ModelName,
                Value = model.Id.ToString(),
                Selected = model.Id == selectedModelCatalogId
            })
            .ToList();
    }

    private static AiProviderConfigurationViewModel MapViewModel(AiProviderConfigurationDto provider)
    {
        var model = new AiProviderConfigurationViewModel
        {
            Id = provider.Id,
            ModelCatalogId = provider.ModelCatalogId,
            ProviderName = provider.ProviderName,
            ModelName = provider.ModelName,
            Endpoint = provider.Endpoint,
            ApiProtocol = provider.ApiProtocol,
            ApiKeyEnvironmentVariable = provider.ApiKeyEnvironmentVariable,
            IsApiKeyConfigured = provider.IsApiKeyConfigured,
            SupportsVision = provider.SupportsVision,
            TimeoutSeconds = provider.TimeoutSeconds,
            IsActive = provider.IsActive,
            IsEnabled = provider.IsEnabled
        };

        model.ApiKeyStatus = BuildBadge(provider.IsApiKeyConfigured, "Configurada", "No configurada");
        model.VisionStatus = IsCatalogBackedProvider(provider.ProviderName)
            ? BuildBadge(provider.SupportsVision, "Modelo con vision", "Modelo sin vision")
            : BuildBadge(provider.SupportsVision, "Vision", "Sin vision");
        model.ActiveStatus = BuildBadge(provider.IsActive, "Activo", "Inactivo");
        model.EnabledStatus = BuildBadge(provider.IsEnabled, "Habilitado", "Deshabilitado");
        model.Task = BuildActionMenu(provider);

        return model;
    }

    private static AiProviderConfigurationDto ToDto(AiProviderConfigurationViewModel model)
    {
        return new AiProviderConfigurationDto
        {
            Id = model.Id,
            ModelCatalogId = model.ModelCatalogId,
            ProviderName = model.ProviderName?.Trim() ?? string.Empty,
            ModelName = model.ModelName?.Trim() ?? string.Empty,
            Endpoint = string.IsNullOrWhiteSpace(model.Endpoint) ? null : model.Endpoint.Trim(),
            ApiProtocol = model.ApiProtocol?.Trim() ?? string.Empty,
            ApiKeyEnvironmentVariable = model.ApiKeyEnvironmentVariable?.Trim() ?? string.Empty,
            SupportsVision = model.SupportsVision,
            TimeoutSeconds = model.TimeoutSeconds,
            IsActive = model.IsActive,
            IsEnabled = model.IsEnabled
        };
    }

    private static string BuildActionMenu(AiProviderConfigurationDto provider)
    {
        var actions = new List<ActionOptionMenuModel>
        {
            new()
            {
                ActionType = ActionType.Edit,
                JavaScriptAction = "showModalForUpdate"
            }
        };

        if (provider.IsEnabled && !provider.IsActive)
        {
            actions.Add(new ActionOptionMenuModel
            {
                ActionType = ActionType.Activate,
                JavaScriptAction = "showModalForActivate"
            });
        }

        if (provider.IsEnabled)
        {
            actions.Add(new ActionOptionMenuModel
            {
                ActionType = ActionType.Deactivate,
                JavaScriptAction = "showModalForDeactivate"
            });
        }

        return ActionButtonHelper.GenerateActionMenu(new ActionMenuModel
        {
            Id = provider.Id.ToString(),
            ActionOptionMenus = actions
        });
    }

    private static string BuildSearchText(AiProviderConfigurationViewModel provider)
    {
        return string.Join(" ", [
            provider.Id.ToString(),
            provider.ProviderName,
            provider.ModelName,
            provider.ApiKeyEnvironmentVariable,
            provider.IsApiKeyConfigured ? "Configurada" : "No configurada",
            provider.SupportsVision ? "Vision" : "Sin vision",
            provider.IsActive ? "Activo" : "Inactivo",
            provider.IsEnabled ? "Habilitado" : "Deshabilitado"
        ]);
    }

    private static string BuildBadge(bool isPositive, string positiveText, string negativeText)
    {
        var cssClass = isPositive
            ? "inline-flex items-center rounded-full bg-green-500/10 text-green-400 px-2.5 py-1 text-xs font-medium"
            : "inline-flex items-center rounded-full bg-red-500/10 text-red-400 px-2.5 py-1 text-xs font-medium";

        return $"<span class=\"{cssClass}\">{(isPositive ? positiveText : negativeText)}</span>";
    }
}
