using System.Text.Json;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure;

namespace Application.Services;

public class TradeValidationOrchestrator : ITradeValidationOrchestrator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAiVisionClientFactory _aiVisionClientFactory;
    private readonly ITradeSetupNormalizer _tradeSetupNormalizer;
    private readonly IStrategyRuleEngine _strategyRuleEngine;
    private readonly IHistoricalEvidenceService _historicalEvidenceService;
    private readonly AiValidationResultFactory _resultFactory;
    private readonly IAiTradeValidationRepository _aiTradeValidationRepository;

    public TradeValidationOrchestrator(
        IAiVisionClientFactory aiVisionClientFactory,
        ITradeSetupNormalizer tradeSetupNormalizer,
        IStrategyRuleEngine strategyRuleEngine,
        IHistoricalEvidenceService historicalEvidenceService,
        AiValidationResultFactory resultFactory,
        IAiTradeValidationRepository aiTradeValidationRepository)
    {
        _aiVisionClientFactory = aiVisionClientFactory;
        _tradeSetupNormalizer = tradeSetupNormalizer;
        _strategyRuleEngine = strategyRuleEngine;
        _historicalEvidenceService = historicalEvidenceService;
        _resultFactory = resultFactory;
        _aiTradeValidationRepository = aiTradeValidationRepository;
    }

    public async Task<AiValidationResultDto> ValidateAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(images);

        cancellationToken.ThrowIfCancellationRequested();

        var client = _aiVisionClientFactory.CreateActiveClient();
        var extraction = await client.ExtractSetupAsync(request, images, cancellationToken);
        var setup = await _tradeSetupNormalizer.NormalizeAsync(request, extraction, cancellationToken);
        var historicalEvidence = await _historicalEvidenceService.GetEvidenceAsync(setup, cancellationToken);
        var rules = _strategyRuleEngine.Evaluate(setup);

        var result = CreateResult(client, extraction, setup, historicalEvidence, rules);

        _resultFactory.ApplyTradingScore(result, setup);

        if (CanSaveCompletedValidation(setup, rules))
        {
            var validation = CreateValidationEntity(request, setup, result);
            var ruleEntities = rules.Select(CreateRuleEntity);

            await _aiTradeValidationRepository.SaveCompletedAsync(validation, ruleEntities, cancellationToken);
        }

        return result;
    }

    private static AiValidationResultDto CreateResult(
        IAiVisionClient client,
        AiVisionExtractionDto extraction,
        NormalizedTradeSetupDto setup,
        HistoricalEvidenceDto? historicalEvidence,
        IReadOnlyList<AiValidationRuleResultDto> rules)
    {
        return new AiValidationResultDto
        {
            ValidationStatus = ResolveValidationStatus(rules),
            ProviderName = client.ProviderName,
            ModelName = client.ModelName,
            PromptVersion = client.PromptVersion,
            SchemaVersion = client.SchemaVersion,
            ModelResponseJson = JsonSerializer.Serialize(extraction, JsonOptions),
            FinalSummary = BuildFinalSummary(rules, historicalEvidence),
            DetectedValues = extraction,
            HistoricalEvidence = historicalEvidence,
            RiskRewardRatio = setup.RiskRewardRatio,
            Rules = rules
        };
    }

    private static AiTradeValidation CreateValidationEntity(
        CreateAiValidationDto request,
        NormalizedTradeSetupDto setup,
        AiValidationResultDto result)
    {
        return new AiTradeValidation
        {
            UserId = request.UserId,
            InstrumentId = setup.InstrumentId,
            DirectionId = setup.DirectionId,
            EntryPrice = setup.EntryPrice,
            StopLoss = setup.StopLoss,
            TakeProfit = setup.TakeProfit,
            UserComment = request.UserComment,
            DetectedTriggerId = setup.TriggerId,
            DetectedSceneryId = setup.SceneryId,
            DetectedFigureId = setup.FigureId,
            DetectedFrameId = setup.FrameId,
            DetectedStageId = setup.StageId,
            DetectedLocationType = setup.LocationType,
            DetectedConfirmationType = setup.ConfirmationType,
            DetectedIsTrendAligned = setup.IsTrendAligned,
            DetectedIsPivotZone = setup.IsPivotZone,
            RiskRewardRatio = setup.RiskRewardRatio,
            StructuralScore = result.StructuralScore,
            TotalScore = result.TotalScore,
            Grade = result.Grade,
            VisualConfidence = setup.VisualConfidence,
            ValidationStatus = result.ValidationStatus.ToString(),
            ProviderName = result.ProviderName,
            ModelName = result.ModelName,
            PromptVersion = result.PromptVersion,
            SchemaVersion = result.SchemaVersion,
            ModelResponseJson = result.ModelResponseJson,
            FinalSummary = result.FinalSummary,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static AiTradeValidationRule CreateRuleEntity(AiValidationRuleResultDto rule)
    {
        return new AiTradeValidationRule
        {
            RuleCode = rule.RuleCode,
            RuleName = rule.RuleName,
            Result = rule.Result.ToString(),
            Weight = rule.Weight,
            ScoreObtained = rule.ScoreObtained,
            Evidence = rule.Evidence,
            Source = rule.Source.ToString()
        };
    }

    private static AiValidationStatus ResolveValidationStatus(IReadOnlyList<AiValidationRuleResultDto> rules)
    {
        if (rules.Any(rule => rule.Result == ValidationRuleResult.Failed))
        {
            return AiValidationStatus.Invalid;
        }

        if (rules.All(rule => rule.Result == ValidationRuleResult.NotConfirmable))
        {
            return AiValidationStatus.InsufficientEvidence;
        }

        if (rules.Any(rule => rule.Result == ValidationRuleResult.NotConfirmable))
        {
            return AiValidationStatus.ConditionallyValid;
        }

        return AiValidationStatus.Valid;
    }

    private static string BuildFinalSummary(
        IReadOnlyList<AiValidationRuleResultDto> rules,
        HistoricalEvidenceDto? historicalEvidence)
    {
        var passed = rules.Count(rule => rule.Result == ValidationRuleResult.Passed);
        var failed = rules.Count(rule => rule.Result == ValidationRuleResult.Failed);
        var notConfirmable = rules.Count(rule => rule.Result == ValidationRuleResult.NotConfirmable);
        var notApplicable = rules.Count(rule => rule.Result == ValidationRuleResult.NotApplicable);

        var ruleSummary = $"Reglas evaluadas: {passed} cumplidas, {failed} incumplidas, {notConfirmable} no confirmables, {notApplicable} no aplicables.";

        return historicalEvidence is null
            ? $"{ruleSummary} Evidencia historica: no disponible."
            : $"{ruleSummary} Evidencia historica: {historicalEvidence.Trades} trades, TP1 {historicalEvidence.TP1Rate:0.##}%, TP2 {historicalEvidence.TP2Rate:0.##}%, TP3 {historicalEvidence.TP3Rate:0.##}%, SL {historicalEvidence.SLRate:0.##}%, score {historicalEvidence.Score:0.##}, muestra {(historicalEvidence.IsSampleSmall ? "insuficiente" : "suficiente")} (minimo {historicalEvidence.MinTrades}).";
    }

    private static bool CanSaveCompletedValidation(
        NormalizedTradeSetupDto setup,
        IReadOnlyList<AiValidationRuleResultDto> rules)
    {
        return setup.TriggerId.HasValue &&
            setup.SceneryId.HasValue &&
            setup.FigureId.HasValue &&
            setup.FrameId.HasValue &&
            setup.StageId.HasValue &&
            setup.LocationType.HasValue &&
            setup.ConfirmationType.HasValue &&
            setup.IsTrendAligned.HasValue &&
            setup.IsPivotZone.HasValue &&
            setup.RiskRewardRatio.HasValue &&
            rules.Count > 0;
    }
}
