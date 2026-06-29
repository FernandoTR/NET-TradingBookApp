using Application.DTOs.AiValidation;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AiValidationResultFactory
{
    private readonly ITradingScoreEngineService _tradingScoreEngineService;

    public AiValidationResultFactory(ITradingScoreEngineService tradingScoreEngineService)
    {
        _tradingScoreEngineService = tradingScoreEngineService;
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
