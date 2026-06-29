using Application.DTOs.AiValidation;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;

namespace Application.Tests;

public class TradeSetupNormalizerTests
{
    [Fact]
    public async Task NormalizeAsync_CalculatesLongRiskReward()
    {
        var normalizer = CreateNormalizer(new CatDirection { Id = 1, Code = "LONG" });
        var request = CreateRequest(directionId: 1, entryPrice: 100m, stopLoss: 90m, takeProfit: 130m);

        var setup = await normalizer.NormalizeAsync(request, new AiVisionExtractionDto(), CancellationToken.None);

        Assert.Equal(3m, setup.RiskRewardRatio);
    }

    [Fact]
    public async Task NormalizeAsync_CalculatesShortRiskReward()
    {
        var normalizer = CreateNormalizer(new CatDirection { Id = 2, Code = "SHORT" });
        var request = CreateRequest(directionId: 2, entryPrice: 100m, stopLoss: 110m, takeProfit: 80m);

        var setup = await normalizer.NormalizeAsync(request, new AiVisionExtractionDto(), CancellationToken.None);

        Assert.Equal(2m, setup.RiskRewardRatio);
    }

    [Fact]
    public async Task NormalizeAsync_RejectsInvalidStopLoss()
    {
        var normalizer = CreateNormalizer(new CatDirection { Id = 1, Code = "LONG" });
        var request = CreateRequest(directionId: 1, entryPrice: 100m, stopLoss: 100m, takeProfit: 130m);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => normalizer.NormalizeAsync(request, new AiVisionExtractionDto(), CancellationToken.None));

        Assert.Equal("stopLoss", exception.ParamName);
    }

    [Fact]
    public async Task NormalizeAsync_RejectsInvalidTakeProfit()
    {
        var normalizer = CreateNormalizer(new CatDirection { Id = 1, Code = "LONG" });
        var request = CreateRequest(directionId: 1, entryPrice: 100m, stopLoss: 90m, takeProfit: 100m);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => normalizer.NormalizeAsync(request, new AiVisionExtractionDto(), CancellationToken.None));

        Assert.Equal("takeProfit", exception.ParamName);
    }

    private static TradeSetupNormalizer CreateNormalizer(CatDirection direction)
    {
        return new TradeSetupNormalizer(
            new InstrumentCatalogService(new CatInstrument { Id = 1, Name = "Test", Ticker = "TST", InstrumentType = "Stock", Currency = "USD", Market = "NYSE" }),
            new DirectionCatalogService(direction),
            new TriggerCatalogService(),
            new SceneryCatalogService(),
            new FigureCatalogService(),
            new FrameCatalogService(),
            new StageCatalogService());
    }

    private static CreateAiValidationDto CreateRequest(
        int directionId,
        decimal entryPrice,
        decimal stopLoss,
        decimal takeProfit)
    {
        return new CreateAiValidationDto
        {
            UserId = "user-1",
            InstrumentId = 1,
            DirectionId = directionId,
            EntryPrice = entryPrice,
            StopLoss = stopLoss,
            TakeProfit = takeProfit
        };
    }
}

public class StrategyRuleEngineTests
{
    [Fact]
    public void Evaluate_ReturnsNotConfirmableForNullFields()
    {
        var engine = new StrategyRuleEngine(
            Microsoft.Extensions.Options.Options.Create(new Application.Common.AiTradeValidationOptions()),
            new SceneryCatalogService());

        var rules = engine.Evaluate(new NormalizedTradeSetupDto());

        Assert.Contains(rules, rule => rule.RuleCode == "RR_MIN" && rule.Result == ValidationRuleResult.NotConfirmable);
        Assert.Contains(rules, rule => rule.RuleCode == "STAGE1_SCENERY" && rule.Result == ValidationRuleResult.NotConfirmable);
        Assert.Contains(rules, rule => rule.RuleCode == "VALID_ZONE_REQUIRED" && rule.Result == ValidationRuleResult.NotConfirmable);
        Assert.Contains(rules, rule => rule.RuleCode == "TRIGGER_REQUIRED" && rule.Result == ValidationRuleResult.NotConfirmable);
        Assert.Contains(rules, rule => rule.RuleCode == "TREND_ALIGNED" && rule.Result == ValidationRuleResult.NotConfirmable);
        Assert.Contains(rules, rule => rule.RuleCode == "CONFIRMATION_COMPATIBLE" && rule.Result == ValidationRuleResult.NotConfirmable);
    }

    [Fact]
    public void Evaluate_AppliesStageOneAllowedSceneryRule()
    {
        var engine = new StrategyRuleEngine(
            Microsoft.Extensions.Options.Options.Create(new Application.Common.AiTradeValidationOptions()),
            new SceneryCatalogService(new CatScenery { Id = 10, Code = "BA" }));

        var rules = engine.Evaluate(new NormalizedTradeSetupDto
        {
            RiskRewardRatio = 2m,
            StageId = 1,
            SceneryId = 10,
            LocationType = (byte)LocationType.Support,
            TriggerId = 5,
            IsTrendAligned = true,
            ConfirmationType = (byte)ConfirmationType.ReversalBreak
        });

        Assert.Contains(rules, rule => rule.RuleCode == "STAGE1_SCENERY" && rule.Result == ValidationRuleResult.Passed);
    }

    [Fact]
    public void Evaluate_FailsStageOneDisallowedSceneryRule()
    {
        var engine = new StrategyRuleEngine(
            Microsoft.Extensions.Options.Options.Create(new Application.Common.AiTradeValidationOptions()),
            new SceneryCatalogService(new CatScenery { Id = 10, Code = "BA" }));

        var rules = engine.Evaluate(new NormalizedTradeSetupDto
        {
            RiskRewardRatio = 2m,
            StageId = 1,
            SceneryId = 99,
            LocationType = (byte)LocationType.Support,
            TriggerId = 5,
            IsTrendAligned = true,
            ConfirmationType = (byte)ConfirmationType.ReversalBreak
        });

        Assert.Contains(rules, rule => rule.RuleCode == "STAGE1_SCENERY" && rule.Result == ValidationRuleResult.Failed);
    }
}

public class AiValidationResultFactoryTests
{
    [Fact]
    public void ApplyTradingScore_UsesTradingScoreEngineService()
    {
        var factory = new AiValidationResultFactory(new TradingScoreEngineService());
        var result = new AiValidationResultDto();
        var setup = new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 1,
            StageId = 2,
            LocationType = (byte)LocationType.Support,
            IsTrendAligned = true,
            ConfirmationType = (byte)ConfirmationType.ReversalRetest,
            IsPivotZone = false
        };

        factory.ApplyTradingScore(result, setup);

        Assert.Equal((short)10, result.StructuralScore);
        Assert.Equal(10, result.TotalScore);
        Assert.Equal(GradeType.A.ToString(), result.Grade);
    }
}

file sealed class InstrumentCatalogService : ICatInstrumentsService
{
    private readonly CatInstrument _instrument;

    public InstrumentCatalogService(CatInstrument instrument)
    {
        _instrument = instrument;
    }

    public Task<bool> AddAsync(CatInstrument entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatInstrument>> GetAllAsync() => Task.FromResult<IEnumerable<CatInstrument>>([_instrument]);
    public Task<CatInstrument?> GetByIdAsync(int id) => Task.FromResult(id == _instrument.Id ? _instrument : null);
    public Task<bool> UpdateAsync(CatInstrument entity) => throw new NotImplementedException();
}

file sealed class DirectionCatalogService : ICatDirectionService
{
    private readonly CatDirection _direction;

    public DirectionCatalogService(CatDirection direction)
    {
        _direction = direction;
    }

    public Task<bool> AddAsync(CatDirection entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatDirection>> GetAllAsync() => Task.FromResult<IEnumerable<CatDirection>>([_direction]);
    public Task<CatDirection?> GetByIdAsync(int id) => Task.FromResult(id == _direction.Id ? _direction : null);
    public Task<bool> UpdateAsync(CatDirection entity) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsDirectionDto>> GetTBAnalyticsDirectionAsync(DTOs.ParametersTBAnalyticsDto parameters) => throw new NotImplementedException();
}

file sealed class TriggerCatalogService : ICatTriggerService
{
    public Task<bool> AddAsync(CatTrigger entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatTrigger>> GetAllAsync() => Task.FromResult<IEnumerable<CatTrigger>>([]);
    public Task<CatTrigger?> GetByIdAsync(int id) => Task.FromResult<CatTrigger?>(new CatTrigger { Id = id, Code = "TRG" });
    public Task<bool> UpdateAsync(CatTrigger entity) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsTriggerDto>> GetTBAnalyticsTriggerAsync(DTOs.ParametersTBAnalyticsDto parameters) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsLastBlockDto>> GetTBAnalyticsLastBlockAsync(DTOs.ParametersAnalyticsDto parameters) => throw new NotImplementedException();
}

file sealed class SceneryCatalogService : ICatSceneryService
{
    private readonly List<CatScenery> _sceneries;

    public SceneryCatalogService(params CatScenery[] sceneries)
    {
        _sceneries = sceneries.ToList();
    }

    public Task<bool> AddAsync(CatScenery entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatScenery>> GetAllAsync() => Task.FromResult<IEnumerable<CatScenery>>(_sceneries);
    public Task<CatScenery?> GetByIdAsync(int id) => Task.FromResult(_sceneries.FirstOrDefault(scenery => scenery.Id == id));
    public Task<bool> UpdateAsync(CatScenery entity) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsSceneryDto>> GetTBAnalyticsSceneryAsync(DTOs.ParametersTBAnalyticsDto parameters) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsSceneryDto>> GetTBAnalyticsSceneryAutoScoreAsync(DTOs.ParametersTBAnalyticsDto parameters) => throw new NotImplementedException();
}

file sealed class FigureCatalogService : ICatFigureService
{
    public Task<bool> AddAsync(CatFigure entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatFigure>> GetAllAsync() => Task.FromResult<IEnumerable<CatFigure>>([]);
    public Task<CatFigure?> GetByIdAsync(int id) => Task.FromResult<CatFigure?>(new CatFigure { Id = id, Code = "FIG" });
    public Task<bool> UpdateAsync(CatFigure entity) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(DTOs.ParametersTBAnalyticsDto parameters) => throw new NotImplementedException();
}

file sealed class FrameCatalogService : ICatFrameService
{
    public Task<bool> AddAsync(CatFrame entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatFrame>> GetAllAsync() => Task.FromResult<IEnumerable<CatFrame>>([]);
    public Task<CatFrame?> GetByIdAsync(int id) => Task.FromResult<CatFrame?>(new CatFrame { Id = id, Code = "FRM" });
    public Task<bool> UpdateAsync(CatFrame entity) => throw new NotImplementedException();
}

file sealed class StageCatalogService : ICatStageService
{
    public Task<bool> AddAsync(CatStage entity) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<IEnumerable<CatStage>> GetAllAsync() => Task.FromResult<IEnumerable<CatStage>>([]);
    public Task<CatStage?> GetByIdAsync(int id) => Task.FromResult<CatStage?>(new CatStage { Id = id, Code = "STG" });
    public Task<bool> UpdateAsync(CatStage entity) => throw new NotImplementedException();
    public Task<List<DTOs.GetTBAnalyticsStageDto>> GetTBAnalyticsStageAsync(DTOs.ParametersTBAnalyticsDto parameters) => throw new NotImplementedException();
}
