using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AiValidationResultFactory
{
    private readonly ITradingScoreEngineService _tradingScoreEngineService;
    private readonly IContextualScoreInputAdapter? _contextualScoreInputAdapter;

    public AiValidationResultFactory(ITradingScoreEngineService tradingScoreEngineService, IContextualScoreInputAdapter? contextualScoreInputAdapter = null)
    {
        _tradingScoreEngineService = tradingScoreEngineService ?? throw new ArgumentNullException(nameof(tradingScoreEngineService));
        _contextualScoreInputAdapter = contextualScoreInputAdapter;
    }

    public void ApplyTradingScore(AiValidationResultDto result, NormalizedTradeSetupDto setup)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(setup);

        var temporaryOrder = BuildTemporaryOrder(setup);

        _tradingScoreEngineService.Evaluate(temporaryOrder);

        result.StructuralScore = temporaryOrder.StructuralScore;
        result.TotalScore = temporaryOrder.TotalScore.HasValue
            ? Convert.ToInt32(temporaryOrder.TotalScore.Value)
            : null;
        result.Grade = temporaryOrder.Grade;
    }

    /// <summary>Appends a contextual evaluation without overwriting the original legacy score or previous contextual results.</summary>
    /// <param name="result">The result retaining the original legacy fields.</param>
    /// <param name="input">The explicit contextual evidence used for the new calculation.</param>
    /// <returns>The new evaluation, preserving decimal precision and its complete trace.</returns>
    public ContextualScoreEvaluationDto ApplyContextualTradingScore(AiValidationResultDto result, ContextualScoreInputDto input)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(input);
        var evaluation = _tradingScoreEngineService.Evaluate(input);
        result.ContextualEvaluations = [.. result.ContextualEvaluations, evaluation];
        return evaluation;
    }

    /// <summary>Adapts the available legacy setup evidence and appends its independently versioned contextual result.</summary>
    /// <param name="result">The result retaining the original legacy evaluation.</param>
    /// <param name="setup">The normalized extraction; missing structural evidence remains unknown.</param>
    /// <param name="cancellationToken">Cancellation for adaptation and catalog lookups.</param>
    /// <returns>The new contextual evaluation without conversion to legacy integer score fields.</returns>
    public async Task<ContextualScoreEvaluationDto> ApplyContextualTradingScoreAsync(
        AiValidationResultDto result, NormalizedTradeSetupDto setup, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(setup);
        cancellationToken.ThrowIfCancellationRequested();
        var adapter = _contextualScoreInputAdapter ?? throw new InvalidOperationException("Contextual input adaptation must be configured.");
        var input = await adapter.FromValidationAsync(setup, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return ApplyContextualTradingScore(result, input);
    }

    private static Order BuildTemporaryOrder(NormalizedTradeSetupDto setup)
    {
        return new Order
        {
            CatInstrumentId = setup.InstrumentId,
            CatDirectionId = setup.DirectionId,
            CatStageId = setup.StageId ?? 0,
            CatFigureId = setup.FigureId ?? 0,
            CatFrameId = setup.FrameId ?? 0,
            CatTriggerId = setup.TriggerId ?? 0,
            CatSceneryId = setup.SceneryId ?? 0,
            LocationType = setup.LocationType,
            ConfirmationType = setup.ConfirmationType,
            IsTrendAligned = setup.IsTrendAligned,
            IsPivotZone = setup.IsPivotZone
        };
    }
}
