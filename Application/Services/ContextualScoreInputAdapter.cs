using System.Globalization;
using System.Text;
using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure;

namespace Application.Services;

/// <summary>Builds conservative score inputs from existing order and extraction contracts.</summary>
/// <param name="figureResolver">The versioned resolver for actual figure-catalog records.</param>
/// <param name="directionService">The actual direction catalog, independently of numeric IDs.</param>
public sealed class ContextualScoreInputAdapter(
    IContextualFigureResolver figureResolver,
    ICatDirectionService directionService) : IContextualScoreInputAdapter
{
    private readonly IContextualFigureResolver _figureResolver = figureResolver ?? throw new ArgumentNullException(nameof(figureResolver));
    private readonly ICatDirectionService _directionService = directionService ?? throw new ArgumentNullException(nameof(directionService));

    /// <inheritdoc />
    public Task<ContextualScoreInputDto> FromOrderAsync(Order order, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        return AdaptAsync(order.CatFigureId, order.CatDirectionId,
            order.Id > 0 ? ValidationSource.HistoricalEvidence : ValidationSource.UserInput, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ContextualScoreInputDto> FromValidationAsync(NormalizedTradeSetupDto setup, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(setup);
        return AdaptAsync(setup.FigureId, setup.DirectionId, ValidationSource.AiVision, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ContextualScoreInputDto> FromValidationAsync(AiTradeValidation validation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(validation);
        var confirmedFigure = validation.ConfirmedFigureId is > 0;
        return AdaptAsync(confirmedFigure ? validation.ConfirmedFigureId : validation.DetectedFigureId,
            validation.DirectionId, confirmedFigure ? ValidationSource.UserConfirmation : ValidationSource.AiVision, cancellationToken);
    }

    private async Task<ContextualScoreInputDto> AdaptAsync(int? figureId, int directionId, ValidationSource source, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hasFigureId = figureId is > 0;
        var reference = new CatalogReferenceDto(
            hasFigureId ? EvidenceAvailability.Present : EvidenceAvailability.Unknown, null, null,
            hasFigureId
                ? new(EvidenceAvailability.Present, figureId, source, null, null, null, null)
                : Unknown<int>("LEGACY_FIGURE_NOT_RECORDED"),
            source, null, null, hasFigureId ? null : "LEGACY_FIGURE_NOT_RECORDED");
        var figure = await _figureResolver.ResolveAsync(reference, cancellationToken);
        var direction = await ResolveDirectionAsync(directionId, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // A single setup frame, stage, location or trend-alignment flag does not identify a structural observation.
        // The recorded values remain in the source order/extraction; they do not prove origin frames, map activity,
        // reviewed modifiers or convergence, and IsPivotZone is not the pivot-line catalog figure.
        var evidence = new ContextualEvidenceDto(reference, null, [], Unknown<bool>("LEGACY_STRUCTURES_NOT_RECORDED"),
            [], Unknown<bool>("LEGACY_CONVERGENCE_REVIEW_NOT_RECORDED"), [], Unknown<bool>("LEGACY_FACTOR_REVIEW_NOT_RECORDED"));
        return new ContextualScoreInputDto(figure, direction, Unknown<ContextualBaseContext>("LEGACY_BASE_CONTEXT_NOT_RECORDED"),
            evidence, Unknown<bool>("LEGACY_CONVERGENCE_ASSESSMENT_NOT_RECORDED"));
    }

    private async Task<EvidenceValueDto<TradeDirection>> ResolveDirectionAsync(int directionId, CancellationToken cancellationToken)
    {
        if (directionId <= 0) return Unknown<TradeDirection>("LEGACY_DIRECTION_NOT_RECORDED");
        cancellationToken.ThrowIfCancellationRequested();
        var direction = await _directionService.GetByIdAsync(directionId);
        cancellationToken.ThrowIfCancellationRequested();
        if (direction is null) return Unknown<TradeDirection>("DIRECTION_CATALOG_NOT_FOUND");

        var names = new[] { Normalize(direction.Code), Normalize(direction.Description) };
        var isLong = names.Any(name => name is "LONG" or "LARGO" or "BUY" or "COMPRA");
        var isShort = names.Any(name => name is "SHORT" or "CORTO" or "SELL" or "VENTA");
        if (isLong == isShort) return Unknown<TradeDirection>("DIRECTION_CATALOG_NOT_DETERMINABLE");
        return new(EvidenceAvailability.Present, isLong ? TradeDirection.Long : TradeDirection.Short,
            ValidationSource.DeterministicRule, null, DateTimeOffset.UtcNow, null, "DIRECTION_CATALOG_RESOLVED");
    }

    private static string Normalize(string? value)
    {
        var builder = new StringBuilder();
        foreach (var character in (value ?? string.Empty).Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
                builder.Append(char.ToUpperInvariant(character));
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static EvidenceValueDto<T> Unknown<T>(string reason) where T : struct =>
        new(EvidenceAvailability.Unknown, null, null, null, null, null, reason);
}
