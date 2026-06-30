using Application.DTOs.AiValidation;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using Web.Models;
using Web.Models.Enums;
using Web.Services;

namespace Web.Controllers;

[Authorize]
public class TradeAssistantController : Controller
{
    private const string HistoricalEvidenceTempDataKeyPrefix = "TradeAssistantHistoricalEvidence:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;
    private readonly IAiValidationImageValidator _aiValidationImageValidator;
    private readonly ITradeValidationOrchestrator _tradeValidationOrchestrator;
    private readonly IAiTradeValidationRepository _aiTradeValidationRepository;
    private readonly IAiTradeValidationMetricService _aiTradeValidationMetricService;
    private readonly ICatCategoryService _catCategoryService;
    private readonly ICatAccountTypeService _catAccountTypeService;
    private readonly ICatInstrumentsService _catInstrumentsService;
    private readonly ICatDirectionService _catDirectionService;
    private readonly ICatFrameService _catFrameService;
    private readonly ICatDayService _catDayService;
    private readonly ICatSceneryService _catSceneryService;
    private readonly ICatStageService _catStageService;
    private readonly ICatFigureService _catFigureService;
    private readonly ICatTriggerService _catTriggerService;

    public TradeAssistantController(
        IIdentityService identityService,
        ILogService logService,
        IAiValidationImageValidator aiValidationImageValidator,
        ITradeValidationOrchestrator tradeValidationOrchestrator,
        IAiTradeValidationRepository aiTradeValidationRepository,
        IAiTradeValidationMetricService aiTradeValidationMetricService,
        ICatCategoryService catCategoryService,
        ICatAccountTypeService catAccountTypeService,
        ICatInstrumentsService catInstrumentsService,
        ICatDirectionService catDirectionService,
        ICatFrameService catFrameService,
        ICatDayService catDayService,
        ICatSceneryService catSceneryService,
        ICatStageService catStageService,
        ICatFigureService catFigureService,
        ICatTriggerService catTriggerService)
    {
        _identityService = identityService;
        _logService = logService;
        _aiValidationImageValidator = aiValidationImageValidator;
        _tradeValidationOrchestrator = tradeValidationOrchestrator;
        _aiTradeValidationRepository = aiTradeValidationRepository;
        _aiTradeValidationMetricService = aiTradeValidationMetricService;
        _catCategoryService = catCategoryService;
        _catAccountTypeService = catAccountTypeService;
        _catInstrumentsService = catInstrumentsService;
        _catDirectionService = catDirectionService;
        _catFrameService = catFrameService;
        _catDayService = catDayService;
        _catSceneryService = catSceneryService;
        _catStageService = catStageService;
        _catFigureService = catFigureService;
        _catTriggerService = catTriggerService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return redirect;
        }

        var model = new TradeAssistantCreateViewModel();
        await PopulateCatalogsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(TradeAssistantCreateViewModel model, CancellationToken cancellationToken)
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return redirect;
        }

        ValidateTradeProposal(model);
        ValidateImageRows(model);

        if (!ModelState.IsValid)
        {
            await PopulateCatalogsAsync(model);
            return View(nameof(Index), model);
        }

        IReadOnlyList<AiValidationImageInputDto> images = [];

        try
        {
            var userId = _identityService.GetCurrentUserId();
            var startedAt = DateTime.UtcNow;
            var request = MapToCreateAiValidationDto(model, userId);
            var uploads = await MapToImageUploadsAsync(model, cancellationToken);

            images = await AiValidationImageInputMapper.MapAsync(uploads, cancellationToken);
            var imageValidationResult = await _aiValidationImageValidator.ValidateAsync(images, cancellationToken);

            if (!imageValidationResult.Succeeded)
            {
                AddModelErrors(imageValidationResult.Errors);
                await PopulateCatalogsAsync(model);
                return View(nameof(Index), model);
            }

            var result = await _tradeValidationOrchestrator.ValidateAsync(request, images, cancellationToken);
            var validationId = await FindPersistedValidationIdAsync(userId, request, result, startedAt, cancellationToken);

            if (!validationId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "La IA respondio, pero no fue posible guardar una validacion completa. Revise la informacion no confirmable y vuelva a intentarlo con imagenes mas claras.");
                await PopulateCatalogsAsync(model);
                return View(nameof(Index), model);
            }

            StoreHistoricalEvidence(validationId.Value, result.HistoricalEvidence);

            return RedirectToAction(nameof(Result), new { id = validationId.Value });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: TradeAssistant, Action: {nameof(Validate)}", ex);
            ModelState.AddModelError(string.Empty, "No fue posible completar la validacion IA. Intente nuevamente.");
            await PopulateCatalogsAsync(model);
            return View(nameof(Index), model);
        }
        finally
        {
            await AiValidationImageInputMapper.DisposeAsync(images);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(int id, CancellationToken cancellationToken)
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return redirect;
        }

        if (id <= 0)
        {
            return RedirectToAction(nameof(History));
        }

        var userId = _identityService.GetCurrentUserId();
        var validation = await _aiTradeValidationRepository.GetByIdAsync(id, userId, cancellationToken);

        if (validation is null)
        {
            TempData["TradeAssistantMessage"] = "No se encontro la validacion solicitada.";
            return RedirectToAction(nameof(History));
        }

        var model = MapToResultViewModel(validation);
        RestoreHistoricalEvidence(model);
        await PopulateResultCatalogsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ConfirmedAiValidationDto confirmation, CancellationToken cancellationToken)
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return redirect;
        }

        if (confirmation.ValidationId <= 0)
        {
            return RedirectToAction(nameof(History));
        }

        confirmation.UserId = _identityService.GetCurrentUserId();
        confirmation.ConfirmedAt = DateTime.UtcNow;

        try
        {
            var saved = await _aiTradeValidationRepository.ConfirmAsync(confirmation, cancellationToken);
            if (saved)
            {
                await _aiTradeValidationMetricService.CreateInitialMetricAsync(
                    confirmation.ValidationId,
                    confirmation.UserId,
                    cancellationToken);
            }

            TempData["TradeAssistantMessage"] = saved
                ? "Confirmación guardada correctamente."
                : "No se pudo guardar la confirmación solicitada.";
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: TradeAssistant, Action: {nameof(Confirm)}", ex);
            TempData["TradeAssistantMessage"] = "No fue posible guardar la confirmación. Intente nuevamente.";
        }

        return RedirectToAction(nameof(Result), new { id = confirmation.ValidationId });
    }

    [HttpGet]
    public async Task<IActionResult> CreateOrder(int validationId, CancellationToken cancellationToken)
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return redirect;
        }

        if (validationId <= 0)
        {
            return RedirectToAction(nameof(History));
        }

        var userId = _identityService.GetCurrentUserId();
        var validation = await _aiTradeValidationRepository.GetByIdAsync(validationId, userId, cancellationToken);

        if (validation is null)
        {
            TempData["TradeAssistantMessage"] = "No se encontro la validacion solicitada.";
            return RedirectToAction(nameof(History));
        }

        if (validation.OrderId.HasValue)
        {
            TempData["TradeAssistantMessage"] = "La validacion ya tiene una orden vinculada.";
            return RedirectToAction(nameof(Result), new { id = validation.Id });
        }

        if (!HasConfirmedOrderValues(validation))
        {
            TempData["TradeAssistantMessage"] = "Confirma todos los valores detectados antes de crear la orden.";
            return RedirectToAction(nameof(Result), new { id = validation.Id });
        }

        var model = MapToCreateOrderFromValidationViewModel(validation);
        await PopulateCreateOrderCatalogsAsync(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LinkOrder([FromBody] LinkOrderToValidationViewModel model, CancellationToken cancellationToken)
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = "La sesion no es valida para vincular la orden.",
                notificationType = NotificationType.Error
            });
        }

        if (model.ValidationId <= 0 || model.OrderId <= 0)
        {
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = "No se recibieron datos validos para vincular la orden.",
                notificationType = NotificationType.Error
            });
        }

        var userId = _identityService.GetCurrentUserId();
        var validation = await _aiTradeValidationRepository.GetByIdAsync(model.ValidationId, userId, cancellationToken);

        if (validation is null)
        {
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = "No se encontro la validacion solicitada para tu usuario.",
                notificationType = NotificationType.Error
            });
        }

        if (validation.OrderId.HasValue)
        {
            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = "La validacion ya tiene una orden vinculada.",
                notificationType = NotificationType.Warning,
                Code = validation.OrderId.Value
            });
        }

        try
        {
            var linked = await _aiTradeValidationRepository.LinkOrderAsync(model.ValidationId, model.OrderId, userId, cancellationToken);

            return Json(new ResultBackViewModel
            {
                Success = linked,
                Message = linked
                    ? "La validacion quedo vinculada con la orden creada."
                    : "La orden se creo, pero no fue posible vincularla con la validacion.",
                notificationType = linked ? NotificationType.Success : NotificationType.Warning,
                Code = model.OrderId
            });
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: TradeAssistant, Action: {nameof(LinkOrder)}", ex);

            return Json(new ResultBackViewModel
            {
                Success = false,
                Message = "La orden se creo, pero ocurrio un error al vincular la validacion.",
                notificationType = NotificationType.Warning,
                Code = model.OrderId
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken cancellationToken)
    {
        var redirect = EnsureValidUser();
        if (redirect is not null)
        {
            return redirect;
        }

        var userId = _identityService.GetCurrentUserId();
        var validations = await _aiTradeValidationRepository.GetCompletedByUserAsync(userId, cancellationToken);
        var instruments = await GetInstrumentNamesAsync();
        var directions = await GetDirectionNamesAsync();
        var model = validations.Select(validation => MapToHistoryItemViewModel(validation, instruments, directions)).ToList();

        return View(model);
    }

    private async Task PopulateCatalogsAsync(TradeAssistantCreateViewModel model)
    {
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(model.InstrumentId);
        ViewBag.DirectionItems = await GetDirectionListSelect(model.DirectionId);
        ViewBag.FrameItems = await GetFrameListSelect(model.FrameId);
        ViewBag.SceneryItems = await GetSceneryListSelect(model.SceneryId);
        ViewBag.StageItems = await GetStageListSelect(model.StageId);
        ViewBag.FigureItems = await GetFigureListSelect(model.FigureId);
        ViewBag.TriggerItems = await GetTriggerListSelect(model.TriggerId);
        ViewBag.ImageRoleItems = GetImageRoleListSelect();
    }

    private async Task PopulateResultCatalogsAsync(TradeAssistantResultViewModel model)
    {
        ViewBag.TriggerItems = await GetTriggerListSelect(model.Confirmation.TriggerId);
        ViewBag.SceneryItems = await GetSceneryListSelect(model.Confirmation.SceneryId);
        ViewBag.StageItems = await GetStageListSelect(model.Confirmation.StageId);
        ViewBag.FigureItems = await GetFigureListSelect(model.Confirmation.FigureId);
        ViewBag.FrameItems = await GetFrameListSelect(model.Confirmation.FrameId);
        ViewBag.LocationTypeItems = GetLocationTypeListSelect(model.Confirmation.LocationType);
        ViewBag.ConfirmationTypeItems = GetConfirmationTypeListSelect(model.Confirmation.ConfirmationType);
        ViewBag.TrendItems = GetBooleanListSelect(model.Confirmation.IsTrendAligned);
        ViewBag.PivotZoneItems = GetBooleanListSelect(model.Confirmation.IsPivotZone);
    }

    private async Task PopulateCreateOrderCatalogsAsync(CreateOrderFromValidationViewModel model)
    {
        ViewBag.CategoryItems = await GetCategoryListSelect(model.Order.CategoryId);
        ViewBag.AccountTypeItems = await GetAccountTypeListSelect(model.Order.AccountTypeId);
        ViewBag.InstrumentItems = await GetInstrumentsListSelect(model.Order.InstrumentsId);
        ViewBag.DayItems = await GetDayListSelect(model.Order.DayId);
        ViewBag.StageItems = await GetStageListSelect(model.Order.StageId);
        ViewBag.FigureItems = await GetFigureListSelect(model.Order.FigureId);
        ViewBag.FrameItems = await GetFrameListSelect(model.Order.FrameId);
        ViewBag.TriggerItems = await GetTriggerListSelect(model.Order.TriggerId);
        ViewBag.DirectionItems = await GetDirectionListSelect(model.Order.DirectionId);
        ViewBag.SceneryItems = await GetSceneryListSelect(model.Order.SceneryId);
        ViewBag.OrderTypeItems = GetOrderTypeListSelect(model.Order.OrderTypeId);
        ViewBag.TradeTypeItems = GetTradeTypeListSelect(model.Order.TradeTypeId);
        ViewBag.LocationTypeItems = GetLocationTypeListSelect(ToLocationType(model.Order.LocationType));
        ViewBag.ConfirmationTypeItems = GetConfirmationTypeListSelect(ToConfirmationType(model.Order.ConfirmationType));
        ViewBag.TrendItems = GetBooleanListSelect(model.Order.IsTrendAligned);
        ViewBag.PivotZoneItems = GetBooleanListSelect(model.Order.IsPivotZone);
    }

    private void ValidateTradeProposal(TradeAssistantCreateViewModel model)
    {
        if (model.InstrumentId <= 0)
        {
            ModelState.AddModelError(string.Empty, "El instrumento es obligatorio.");
        }

        if (model.DirectionId <= 0)
        {
            ModelState.AddModelError(string.Empty, "La direccion es obligatoria.");
        }

        if (model.EntryPrice <= 0)
        {
            ModelState.AddModelError(string.Empty, "La entrada debe ser mayor a cero.");
        }

        if (model.StopLoss <= 0)
        {
            ModelState.AddModelError(string.Empty, "El stop loss debe ser mayor a cero.");
        }

        if (model.TakeProfit <= 0)
        {
            ModelState.AddModelError(string.Empty, "El take profit debe ser mayor a cero.");
        }
    }

    private void ValidateImageRows(TradeAssistantCreateViewModel model)
    {
        if (model.Images.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debe cargar al menos una imagen.");
            return;
        }

        if (model.Images.Count > 4)
        {
            ModelState.AddModelError(string.Empty, "Solo se permiten hasta 4 imagenes por validacion.");
        }

        for (var index = 0; index < model.Images.Count; index++)
        {
            var image = model.Images[index];
            var displayIndex = index + 1;

            if (image.File is null)
            {
                ModelState.AddModelError(string.Empty, $"La imagen {displayIndex} requiere un archivo.");
            }

            if (!Enum.IsDefined(typeof(TradingImageRole), image.ImageRole))
            {
                ModelState.AddModelError(string.Empty, $"La imagen {displayIndex} requiere un rol valido.");
            }

            if (!image.FrameId.HasValue || image.FrameId.Value <= 0)
            {
                ModelState.AddModelError(string.Empty, $"La imagen {displayIndex} requiere temporalidad.");
            }

            if (image.SortOrder <= 0)
            {
                ModelState.AddModelError(string.Empty, $"La imagen {displayIndex} requiere un orden mayor a cero.");
            }
        }
    }

    private async Task<IReadOnlyList<AiValidationImageUploadViewModel>> MapToImageUploadsAsync(
        TradeAssistantCreateViewModel model,
        CancellationToken cancellationToken)
    {
        var uploads = new List<AiValidationImageUploadViewModel>();

        foreach (var image in model.Images.OrderBy(image => image.SortOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var frameCode = await GetFrameCodeAsync(image.FrameId);

            uploads.Add(new AiValidationImageUploadViewModel
            {
                File = image.File,
                FrameCode = frameCode,
                ImageRole = (TradingImageRole)image.ImageRole,
                SortOrder = image.SortOrder,
                Comment = image.Comment
            });
        }

        return uploads;
    }

    private async Task<string> GetFrameCodeAsync(int? frameId)
    {
        if (!frameId.HasValue || frameId.Value <= 0)
        {
            return string.Empty;
        }

        var frame = await _catFrameService.GetByIdAsync(frameId.Value);
        return frame?.Code ?? string.Empty;
    }

    private static CreateAiValidationDto MapToCreateAiValidationDto(TradeAssistantCreateViewModel model, string userId)
    {
        return new CreateAiValidationDto
        {
            UserId = userId,
            InstrumentId = model.InstrumentId,
            DirectionId = model.DirectionId,
            EntryPrice = model.EntryPrice,
            StopLoss = model.StopLoss,
            TakeProfit = model.TakeProfit,
            UserComment = model.UserComment
        };
    }

    private async Task<int?> FindPersistedValidationIdAsync(
        string userId,
        CreateAiValidationDto request,
        AiValidationResultDto result,
        DateTime startedAt,
        CancellationToken cancellationToken)
    {
        var validations = await _aiTradeValidationRepository.GetByUserAsync(userId, cancellationToken);

        var validation = validations.FirstOrDefault(validation =>
            validation.CreatedAt >= startedAt.AddSeconds(-5) &&
            validation.InstrumentId == request.InstrumentId &&
            validation.DirectionId == request.DirectionId &&
            validation.EntryPrice == request.EntryPrice &&
            validation.StopLoss == request.StopLoss &&
            validation.TakeProfit == request.TakeProfit &&
            validation.ProviderName == result.ProviderName &&
            validation.ModelName == result.ModelName &&
            validation.ModelResponseJson == result.ModelResponseJson);

        return validation?.Id;
    }

    private static TradeAssistantResultViewModel MapToResultViewModel(AiTradeValidation validation)
    {
        var detectedValues = DeserializeDetectedValues(validation.ModelResponseJson);

        return new TradeAssistantResultViewModel
        {
            ValidationId = validation.Id,
            OrderId = validation.OrderId,
            CanCreateOrder = validation.OrderId is null && HasConfirmedOrderValues(validation),
            Result = new AiValidationResultDto
            {
                ValidationStatus = ParseEnum(validation.ValidationStatus, AiValidationStatus.InsufficientEvidence),
                ProviderName = validation.ProviderName,
                ModelName = validation.ModelName,
                PromptVersion = validation.PromptVersion,
                SchemaVersion = validation.SchemaVersion,
                ModelResponseJson = validation.ModelResponseJson,
                FinalSummary = validation.FinalSummary,
                DetectedValues = detectedValues,
                RiskRewardRatio = validation.RiskRewardRatio,
                StructuralScore = validation.StructuralScore,
                TotalScore = validation.TotalScore,
                Grade = validation.Grade,
                Rules = validation.Rules
                    .OrderBy(rule => rule.Id)
                    .Select(MapRule)
                    .ToList()
            },
            Confirmation = new ConfirmedAiValidationDto
            {
                ValidationId = validation.Id,
                UserId = validation.UserId,
                TriggerId = validation.ConfirmedTriggerId ?? validation.DetectedTriggerId,
                SceneryId = validation.ConfirmedSceneryId ?? validation.DetectedSceneryId,
                FigureId = validation.ConfirmedFigureId ?? validation.DetectedFigureId,
                FrameId = validation.ConfirmedFrameId ?? validation.DetectedFrameId,
                StageId = validation.ConfirmedStageId ?? validation.DetectedStageId,
                LocationType = ToLocationType(validation.ConfirmedLocationType ?? validation.DetectedLocationType),
                ConfirmationType = ToConfirmationType(validation.ConfirmedConfirmationType ?? validation.DetectedConfirmationType),
                IsTrendAligned = validation.ConfirmedIsTrendAligned ?? validation.DetectedIsTrendAligned,
                IsPivotZone = validation.ConfirmedIsPivotZone ?? validation.DetectedIsPivotZone,
                ConfirmedAt = validation.ConfirmedAt
            }
        };
    }

    private static CreateOrderFromValidationViewModel MapToCreateOrderFromValidationViewModel(AiTradeValidation validation)
    {
        return new CreateOrderFromValidationViewModel
        {
            ValidationId = validation.Id,
            Order = new OrdersCreateViewModel
            {
                InstrumentsId = validation.InstrumentId,
                StageId = validation.ConfirmedStageId.GetValueOrDefault(),
                FigureId = validation.ConfirmedFigureId.GetValueOrDefault(),
                FrameId = validation.ConfirmedFrameId.GetValueOrDefault(),
                TriggerId = validation.ConfirmedTriggerId.GetValueOrDefault(),
                DirectionId = validation.DirectionId,
                SceneryId = validation.ConfirmedSceneryId.GetValueOrDefault(),
                IsTrendAligned = validation.ConfirmedIsTrendAligned,
                LocationType = validation.ConfirmedLocationType,
                ConfirmationType = validation.ConfirmedConfirmationType,
                IsPivotZone = validation.ConfirmedIsPivotZone
            }
        };
    }

    private static bool HasConfirmedOrderValues(AiTradeValidation validation)
    {
        return validation.InstrumentId > 0 &&
               validation.DirectionId > 0 &&
               validation.ConfirmedAt.HasValue &&
               validation.ConfirmedTriggerId.HasValue &&
               validation.ConfirmedSceneryId.HasValue &&
               validation.ConfirmedFigureId.HasValue &&
               validation.ConfirmedFrameId.HasValue &&
               validation.ConfirmedStageId.HasValue &&
               validation.ConfirmedLocationType.HasValue &&
               validation.ConfirmedConfirmationType.HasValue &&
               validation.ConfirmedIsTrendAligned.HasValue &&
               validation.ConfirmedIsPivotZone.HasValue;
    }

    private static TradeAssistantHistoryItemViewModel MapToHistoryItemViewModel(
        AiTradeValidation validation,
        IReadOnlyDictionary<int, string> instruments,
        IReadOnlyDictionary<int, string> directions)
    {
        return new TradeAssistantHistoryItemViewModel
        {
            ValidationId = validation.Id,
            CreatedAt = validation.CreatedAt,
            Instrument = GetCatalogValue(instruments, validation.InstrumentId),
            Direction = GetCatalogValue(directions, validation.DirectionId),
            ValidationStatus = validation.ValidationStatus,
            TotalScore = validation.TotalScore,
            ProviderName = validation.ProviderName,
            ModelName = validation.ModelName
        };
    }

    private async Task<IReadOnlyDictionary<int, string>> GetInstrumentNamesAsync()
    {
        var instruments = await _catInstrumentsService.GetAllAsync();

        return instruments.ToDictionary(
            instrument => instrument.Id,
            instrument => string.IsNullOrWhiteSpace(instrument.Ticker) ? instrument.Name : instrument.Ticker);
    }

    private async Task<IReadOnlyDictionary<int, string>> GetDirectionNamesAsync()
    {
        var directions = await _catDirectionService.GetAllAsync();

        return directions.ToDictionary(
            direction => direction.Id,
            direction => BuildCatalogText(direction.Code, direction.Description));
    }

    private static string GetCatalogValue(IReadOnlyDictionary<int, string> values, int id)
    {
        return values.TryGetValue(id, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : $"#{id}";
    }

    private void StoreHistoricalEvidence(int validationId, HistoricalEvidenceDto? evidence)
    {
        if (evidence is null)
        {
            return;
        }

        TempData[GetHistoricalEvidenceTempDataKey(validationId)] = JsonSerializer.Serialize(evidence, JsonOptions);
    }

    private void RestoreHistoricalEvidence(TradeAssistantResultViewModel model)
    {
        var key = GetHistoricalEvidenceTempDataKey(model.ValidationId);

        if (TempData[key] is not string serializedEvidence || string.IsNullOrWhiteSpace(serializedEvidence))
        {
            return;
        }

        try
        {
            model.Result.HistoricalEvidence = JsonSerializer.Deserialize<HistoricalEvidenceDto>(serializedEvidence, JsonOptions);
        }
        catch (JsonException)
        {
            model.Result.HistoricalEvidence = null;
        }
    }

    private static string GetHistoricalEvidenceTempDataKey(int validationId)
    {
        return $"{HistoricalEvidenceTempDataKeyPrefix}{validationId}";
    }

    private static AiValidationRuleResultDto MapRule(AiTradeValidationRule rule)
    {
        return new AiValidationRuleResultDto
        {
            RuleCode = rule.RuleCode,
            RuleName = rule.RuleName,
            Result = ParseEnum(rule.Result, ValidationRuleResult.NotConfirmable),
            Weight = rule.Weight,
            ScoreObtained = rule.ScoreObtained,
            Evidence = rule.Evidence,
            Source = ParseEnum(rule.Source, ValidationSource.DeterministicRule)
        };
    }

    private static AiVisionExtractionDto? DeserializeDetectedValues(string modelResponseJson)
    {
        if (string.IsNullOrWhiteSpace(modelResponseJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AiVisionExtractionDto>(modelResponseJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }

    private static LocationType? ToLocationType(byte? value)
    {
        return value.HasValue && Enum.IsDefined(typeof(LocationType), value.Value)
            ? (LocationType)value.Value
            : null;
    }

    private static ConfirmationType? ToConfirmationType(byte? value)
    {
        return value.HasValue && Enum.IsDefined(typeof(ConfirmationType), value.Value)
            ? (ConfirmationType)value.Value
            : null;
    }

    private void AddModelErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    public async Task<List<SelectListItem>> GetInstrumentsListSelect(int? selectedId)
    {
        var data = await _catInstrumentsService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.OrderBy(o => o.Ticker).Select(x => new SelectListItem
        {
            Text = x.Ticker,
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetCategoryListSelect(int? selectedId)
    {
        var data = await _catCategoryService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = x.Name ?? string.Empty,
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetAccountTypeListSelect(int? selectedId)
    {
        var data = await _catAccountTypeService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText(x.Code, x.Description),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetDayListSelect(int? selectedId)
    {
        var data = await _catDayService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText(x.Code, x.Description),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetDirectionListSelect(int? selectedId)
    {
        var data = await _catDirectionService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText("",x.Description),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetFrameListSelect(int? selectedId)
    {
        var data = await _catFrameService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText(x.Code,""),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetSceneryListSelect(int? selectedId)
    {
        var data = await _catSceneryService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText(x.Code, ""),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetStageListSelect(int? selectedId)
    {
        var data = await _catStageService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText("", x.Description),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetFigureListSelect(int? selectedId)
    {
        var data = await _catFigureService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText(x.Code, x.Description),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public async Task<List<SelectListItem>> GetTriggerListSelect(int? selectedId)
    {
        var data = await _catTriggerService.GetAllAsync();

        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(data.Select(x => new SelectListItem
        {
            Text = BuildCatalogText(x.Code, x.Description),
            Value = x.Id.ToString(),
            Selected = IsSelected(selectedId, x.Id)
        }));

        return selectItems;
    }

    public static List<SelectListItem> GetImageRoleListSelect()
    {
        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(Enum.GetValues<TradingImageRole>().Select(role => new SelectListItem
        {
            Text = GetImageRoleText(role),
            Value = ((int)role).ToString()
        }));

        return selectItems;
    }

    public static List<SelectListItem> GetLocationTypeListSelect(LocationType? selectedValue)
    {
        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(Enum.GetValues<LocationType>().Select(value => new SelectListItem
        {
            Text = GetLocationTypeText(value),
            Value = ((byte)value).ToString(),
            Selected = selectedValue == value
        }));

        return selectItems;
    }

    public static List<SelectListItem> GetConfirmationTypeListSelect(ConfirmationType? selectedValue)
    {
        var selectItems = CreateEmptySelectList();

        selectItems.AddRange(Enum.GetValues<ConfirmationType>().Select(value => new SelectListItem
        {
            Text = GetConfirmationTypeText(value),
            Value = ((byte)value).ToString(),
            Selected = selectedValue == value
        }));

        return selectItems;
    }

    public static List<SelectListItem> GetBooleanListSelect(bool? selectedValue)
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "Si", Value = "true", Selected = selectedValue == true },
            new SelectListItem { Text = "No", Value = "false", Selected = selectedValue == false }
        };
    }

    public static List<SelectListItem> GetOrderTypeListSelect(string? selectedValue)
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "Market", Value = "Market", Selected = selectedValue == "Market" },
            new SelectListItem { Text = "Limit", Value = "Limit", Selected = selectedValue == "Limit" },
            new SelectListItem { Text = "Stop", Value = "Stop", Selected = selectedValue == "Stop" }
        };
    }

    public static List<SelectListItem> GetTradeTypeListSelect(string? selectedValue)
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" },
            new SelectListItem { Text = "Compra", Value = "Buy", Selected = selectedValue == "Buy" }
        };
    }

    private static List<SelectListItem> CreateEmptySelectList()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" }
        };
    }

    private static bool IsSelected(int? selectedId, int itemId)
    {
        return selectedId.HasValue && selectedId.Value == itemId;
    }

    private static string BuildCatalogText(string? code, string? description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return description ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(description)
            ? code
            : $"{code} - {description}";
    }

    private static string GetImageRoleText(TradingImageRole role)
    {
        return role switch
        {
            TradingImageRole.GeneralContext => "Contexto general",
            TradingImageRole.HigherTimeframe => "Temporalidad mayor",
            TradingImageRole.MainTimeframe => "Temporalidad principal",
            TradingImageRole.EntryTimeframe => "Temporalidad de entrada",
            TradingImageRole.Trigger => "Gatillo",
            TradingImageRole.Confirmation => "Confirmacion",
            TradingImageRole.UserMarkup => "Marcado del usuario",
            _ => role.ToString()
        };
    }

    private static string GetLocationTypeText(LocationType value)
    {
        return value switch
        {
            LocationType.Support => "Soporte",
            LocationType.Middle => "Medio",
            LocationType.Resistance => "Resistencia",
            _ => value.ToString()
        };
    }

    private static string GetConfirmationTypeText(ConfirmationType value)
    {
        return value switch
        {
            ConfirmationType.None => "Sin confirmacion",
            ConfirmationType.ContinuationBreak => "Continuacion break",
            ConfirmationType.ContinuationRetest => "Continuacion retest",
            ConfirmationType.ReversalBreak => "Reversal break",
            ConfirmationType.ReversalRetest => "Reversal retest",
            _ => value.ToString()
        };
    }

    private IActionResult? EnsureValidUser()
    {
        try
        {
            var currentUser = _identityService.GetCurrentUserAsync();

            if (User.Identity?.IsAuthenticated != true || currentUser is null || !currentUser.IsValid)
            {
                return currentUser?.ResetFlag == true
                    ? RedirectToAction("ChangePassword", "Manage")
                    : RedirectToAction("SignIn", "Account");
            }

            return null;
        }
        catch (Exception ex)
        {
            var actionName = ControllerContext.ActionDescriptor.ActionName;
            _logService.ErrorLog($"Controller: TradeAssistant, Action: {actionName}", ex);
            return RedirectToAction("Index", "Home");
        }
    }
}
