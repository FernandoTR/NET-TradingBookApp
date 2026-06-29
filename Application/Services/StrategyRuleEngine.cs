using Application.Common;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class StrategyRuleEngine : IStrategyRuleEngine
{
    private static readonly HashSet<string> StageOneAllowedSceneryCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BA",
        "B",
        "BC"
    };

    private readonly AiTradeValidationOptions _options;
    private readonly HashSet<int> _stageOneAllowedSceneryIds;

    public StrategyRuleEngine(
        IOptions<AiTradeValidationOptions> options,
        ICatSceneryService catSceneryService)
    {
        _options = options.Value;
        _stageOneAllowedSceneryIds = catSceneryService
            .GetAllAsync()
            .GetAwaiter()
            .GetResult()
            .Where(scenery => !string.IsNullOrWhiteSpace(scenery.Code)
                && StageOneAllowedSceneryCodes.Contains(scenery.Code.Trim()))
            .Select(scenery => scenery.Id)
            .ToHashSet();
    }

    public IReadOnlyList<AiValidationRuleResultDto> Evaluate(NormalizedTradeSetupDto setup)
    {
        ArgumentNullException.ThrowIfNull(setup);

        return new List<AiValidationRuleResultDto>
        {
            EvaluateMinimumRiskReward(setup),
            EvaluateStageOneScenery(setup),
            EvaluateRequiredValidZone(setup),
            EvaluateRequiredTrigger(setup),
            EvaluateTrendAlignment(setup),
            EvaluateCompatibleConfirmation(setup)
        };
    }

    private AiValidationRuleResultDto EvaluateMinimumRiskReward(NormalizedTradeSetupDto setup)
    {
        if (!setup.RiskRewardRatio.HasValue)
        {
            return Rule(
                "RR_MIN",
                "RR minimo",
                ValidationRuleResult.NotConfirmable,
                "No se pudo confirmar el RR calculado.");
        }

        return Rule(
            "RR_MIN",
            "RR minimo",
            setup.RiskRewardRatio.Value >= _options.MinimumRiskRewardRatio
                ? ValidationRuleResult.Passed
                : ValidationRuleResult.Failed,
            $"RR calculado {setup.RiskRewardRatio.Value:0.####}. Minimo requerido {_options.MinimumRiskRewardRatio:0.####}.");
    }

    private AiValidationRuleResultDto EvaluateStageOneScenery(NormalizedTradeSetupDto setup)
    {
        if (!setup.StageId.HasValue)
        {
            return Rule(
                "STAGE1_SCENERY",
                "Escenario permitido en etapa 1",
                ValidationRuleResult.NotConfirmable,
                "No se pudo confirmar la etapa.");
        }

        if (setup.StageId.Value != 1)
        {
            return Rule(
                "STAGE1_SCENERY",
                "Escenario permitido en etapa 1",
                ValidationRuleResult.NotApplicable,
                $"La etapa {setup.StageId.Value} no requiere esta regla.");
        }

        if (!setup.SceneryId.HasValue)
        {
            return Rule(
                "STAGE1_SCENERY",
                "Escenario permitido en etapa 1",
                ValidationRuleResult.NotConfirmable,
                "No se pudo confirmar el escenario.");
        }

        return Rule(
            "STAGE1_SCENERY",
            "Escenario permitido en etapa 1",
            _stageOneAllowedSceneryIds.Contains(setup.SceneryId.Value)
                ? ValidationRuleResult.Passed
                : ValidationRuleResult.Failed,
            $"Escenario catalogo Id {setup.SceneryId.Value} evaluado para etapa 1.");
    }

    private static AiValidationRuleResultDto EvaluateRequiredValidZone(NormalizedTradeSetupDto setup)
    {
        if (!setup.LocationType.HasValue)
        {
            return Rule(
                "VALID_ZONE_REQUIRED",
                "Zona valida obligatoria",
                ValidationRuleResult.NotConfirmable,
                "No se pudo confirmar la zona.");
        }

        return Rule(
            "VALID_ZONE_REQUIRED",
            "Zona valida obligatoria",
            Enum.IsDefined(typeof(LocationType), setup.LocationType.Value)
                ? ValidationRuleResult.Passed
                : ValidationRuleResult.Failed,
            $"Zona detectada {(LocationType)setup.LocationType.Value}.");
    }

    private static AiValidationRuleResultDto EvaluateRequiredTrigger(NormalizedTradeSetupDto setup)
    {
        return Rule(
            "TRIGGER_REQUIRED",
            "Gatillo obligatorio",
            setup.TriggerId.HasValue ? ValidationRuleResult.Passed : ValidationRuleResult.NotConfirmable,
            setup.TriggerId.HasValue
                ? $"Gatillo catalogo Id {setup.TriggerId.Value}."
                : "No se pudo confirmar el gatillo.");
    }

    private static AiValidationRuleResultDto EvaluateTrendAlignment(NormalizedTradeSetupDto setup)
    {
        if (!setup.IsTrendAligned.HasValue)
        {
            return Rule(
                "TREND_ALIGNED",
                "Direccion alineada",
                ValidationRuleResult.NotConfirmable,
                "No se pudo confirmar la alineacion con tendencia.");
        }

        return Rule(
            "TREND_ALIGNED",
            "Direccion alineada",
            setup.IsTrendAligned.Value ? ValidationRuleResult.Passed : ValidationRuleResult.Failed,
            setup.IsTrendAligned.Value
                ? "La direccion esta alineada con tendencia."
                : "La direccion no esta alineada con tendencia.");
    }

    private static AiValidationRuleResultDto EvaluateCompatibleConfirmation(NormalizedTradeSetupDto setup)
    {
        if (!setup.ConfirmationType.HasValue)
        {
            return Rule(
                "CONFIRMATION_COMPATIBLE",
                "Confirmacion compatible",
                ValidationRuleResult.NotConfirmable,
                "No se pudo confirmar el tipo de confirmacion.");
        }

        if (!Enum.IsDefined(typeof(ConfirmationType), setup.ConfirmationType.Value))
        {
            return Rule(
                "CONFIRMATION_COMPATIBLE",
                "Confirmacion compatible",
                ValidationRuleResult.Failed,
                $"Confirmacion invalida {(int)setup.ConfirmationType.Value}.");
        }

        var confirmationType = (ConfirmationType)setup.ConfirmationType.Value;

        return Rule(
            "CONFIRMATION_COMPATIBLE",
            "Confirmacion compatible",
            confirmationType == ConfirmationType.None
                ? ValidationRuleResult.Failed
                : ValidationRuleResult.Passed,
            $"Confirmacion detectada {confirmationType}.");
    }

    private static AiValidationRuleResultDto Rule(
        string code,
        string name,
        ValidationRuleResult result,
        string evidence)
    {
        return new AiValidationRuleResultDto
        {
            RuleCode = code,
            RuleName = name,
            Result = result,
            Weight = 1m,
            ScoreObtained = result == ValidationRuleResult.Passed ? 1m : 0m,
            Evidence = evidence,
            Source = ValidationSource.DeterministicRule
        };
    }
}
