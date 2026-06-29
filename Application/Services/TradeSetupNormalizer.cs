using Application.DTOs.AiValidation;
using Application.Interfaces;
using System.Globalization;
using System.Text;

namespace Application.Services;

public class TradeSetupNormalizer : ITradeSetupNormalizer
{
    private readonly ICatInstrumentsService _catInstrumentsService;
    private readonly ICatDirectionService _catDirectionService;
    private readonly ICatTriggerService _catTriggerService;
    private readonly ICatSceneryService _catSceneryService;
    private readonly ICatFigureService _catFigureService;
    private readonly ICatFrameService _catFrameService;
    private readonly ICatStageService _catStageService;

    public TradeSetupNormalizer(
        ICatInstrumentsService catInstrumentsService,
        ICatDirectionService catDirectionService,
        ICatTriggerService catTriggerService,
        ICatSceneryService catSceneryService,
        ICatFigureService catFigureService,
        ICatFrameService catFrameService,
        ICatStageService catStageService)
    {
        _catInstrumentsService = catInstrumentsService;
        _catDirectionService = catDirectionService;
        _catTriggerService = catTriggerService;
        _catSceneryService = catSceneryService;
        _catFigureService = catFigureService;
        _catFrameService = catFrameService;
        _catStageService = catStageService;
    }

    public async Task<NormalizedTradeSetupDto> NormalizeAsync(
        CreateAiValidationDto request,
        AiVisionExtractionDto extraction,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(extraction);

        cancellationToken.ThrowIfCancellationRequested();

        var instrumentId = await ValidateRequiredCatalogIdAsync(
            request.InstrumentId,
            id => _catInstrumentsService.GetByIdAsync(id),
            nameof(request.InstrumentId),
            cancellationToken);

        var direction = await ValidateRequiredCatalogEntityAsync(
            request.DirectionId,
            id => _catDirectionService.GetByIdAsync(id),
            nameof(request.DirectionId),
            cancellationToken);
        var directionKind = ResolveDirectionKind(direction.Code, direction.Description, nameof(request.DirectionId));

        return new NormalizedTradeSetupDto
        {
            InstrumentId = instrumentId,
            DirectionId = request.DirectionId,
            EntryPrice = request.EntryPrice,
            StopLoss = request.StopLoss,
            TakeProfit = request.TakeProfit,
            TriggerId = await NormalizeOptionalCatalogIdAsync(
                extraction.TriggerId,
                id => _catTriggerService.GetByIdAsync(id),
                cancellationToken),
            SceneryId = await NormalizeOptionalCatalogIdAsync(
                extraction.SceneryId,
                id => _catSceneryService.GetByIdAsync(id),
                cancellationToken),
            FigureId = await NormalizeOptionalCatalogIdAsync(
                extraction.FigureId,
                id => _catFigureService.GetByIdAsync(id),
                cancellationToken),
            FrameId = await NormalizeOptionalCatalogIdAsync(
                extraction.FrameId,
                id => _catFrameService.GetByIdAsync(id),
                cancellationToken),
            StageId = await NormalizeOptionalCatalogIdAsync(
                extraction.StageId,
                id => _catStageService.GetByIdAsync(id),
                cancellationToken),
            LocationType = extraction.LocationType.HasValue ? (byte)extraction.LocationType.Value : null,
            ConfirmationType = extraction.ConfirmationType.HasValue ? (byte)extraction.ConfirmationType.Value : null,
            IsTrendAligned = extraction.IsTrendAligned,
            IsPivotZone = extraction.IsPivotZone,
            RiskRewardRatio = CalculateRiskRewardRatio(directionKind, request.EntryPrice, request.StopLoss, request.TakeProfit),
            VisualConfidence = extraction.VisualConfidence ?? 0m
        };
    }

    private static async Task<int> ValidateRequiredCatalogIdAsync<TCatalog>(
        int id,
        Func<int, Task<TCatalog?>> resolver,
        string fieldName,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentException($"{fieldName} must be greater than zero.", fieldName);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var entity = await resolver(id);
        cancellationToken.ThrowIfCancellationRequested();

        if (entity is null)
        {
            throw new ArgumentException($"{fieldName} does not exist in the catalog.", fieldName);
        }

        return id;
    }

    private static async Task<TCatalog> ValidateRequiredCatalogEntityAsync<TCatalog>(
        int id,
        Func<int, Task<TCatalog?>> resolver,
        string fieldName,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentException($"{fieldName} must be greater than zero.", fieldName);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var entity = await resolver(id);
        cancellationToken.ThrowIfCancellationRequested();

        if (entity is null)
        {
            throw new ArgumentException($"{fieldName} does not exist in the catalog.", fieldName);
        }

        return entity;
    }

    private static async Task<int?> NormalizeOptionalCatalogIdAsync<TCatalog>(
        int? id,
        Func<int, Task<TCatalog?>> resolver,
        CancellationToken cancellationToken)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var entity = await resolver(id.Value);
        cancellationToken.ThrowIfCancellationRequested();

        return entity is null ? null : id.Value;
    }

    private static TradeDirectionKind ResolveDirectionKind(string? code, string? description, string fieldName)
    {
        var value = NormalizeDirectionText($"{code} {description}");

        if (ContainsAny(value, "LONG", "LARGO", "BUY", "COMPRA"))
        {
            return TradeDirectionKind.Long;
        }

        if (ContainsAny(value, "SHORT", "CORTO", "SELL", "VENTA"))
        {
            return TradeDirectionKind.Short;
        }

        throw new ArgumentException($"{fieldName} must resolve to Long or Short.", fieldName);
    }

    private static decimal CalculateRiskRewardRatio(
        TradeDirectionKind directionKind,
        decimal entryPrice,
        decimal stopLoss,
        decimal takeProfit)
    {
        var (benefit, risk) = directionKind switch
        {
            TradeDirectionKind.Long => (takeProfit - entryPrice, entryPrice - stopLoss),
            TradeDirectionKind.Short => (entryPrice - takeProfit, stopLoss - entryPrice),
            _ => throw new ArgumentOutOfRangeException(nameof(directionKind))
        };

        if (risk <= 0)
        {
            throw new ArgumentException("Risk must be greater than zero for the selected direction.", nameof(stopLoss));
        }

        if (benefit <= 0)
        {
            throw new ArgumentException("Benefit must be greater than zero for the selected direction.", nameof(takeProfit));
        }

        return benefit / risk;
    }

    private static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(value.Contains);

    private static string NormalizeDirectionText(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private enum TradeDirectionKind
    {
        Long,
        Short
    }
}
