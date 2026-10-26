using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure;

namespace Application.Services;

public class TradingScoreEngineService : ITradingScoreEngineService
{
    private readonly IContextualScoreEngineService _contextualScoreEngine;

    /// <summary>Creates a scoring facade with a compatible default for existing direct callers.</summary>
    /// <param name="contextualScoreEngine">The injected versioned engine, or the default deterministic engine for direct legacy construction.</param>
    public TradingScoreEngineService(IContextualScoreEngineService? contextualScoreEngine = null)
    {
        _contextualScoreEngine = contextualScoreEngine ?? new ContextualScoreEngineService(new ContextualScenarioClassifier());
    }

    /// <inheritdoc />
    public ContextualScoreEvaluationDto Evaluate(ContextualScoreInputDto input) => _contextualScoreEngine.Evaluate(input);

    /// <inheritdoc />
    public void Evaluate(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var score = CalculateStructuralScore(order);

        // 🔴 ETAPA 1 CAP (REGLA CRÍTICA)
        score = ApplyStageOneCap(order.CatStageId, score);

        order.StructuralScore = (short)score;
        order.TotalScore = score;
        order.Grade = GetGrade(order.CatStageId, score).ToString();
    }

    // =========================
    // 🧮 SCORE PRINCIPAL
    // =========================

    private int CalculateStructuralScore(Order o)
    {
        return
            GetLocationScore(o) +
            GetTrendScore(o) +
            GetConfirmationScore(o) +
            GetPivotPenalty(o);
    }

    // =========================
    // 📍 LOCATION
    // =========================

    private int GetLocationScore(Order o) =>
        o.LocationType switch
        {
            (int)LocationType.Support => 4,
            (int)LocationType.Middle => -1,
            (int)LocationType.Resistance => -4,
            null => 0,
            _ => 0
        };

    // =========================
    // 📈 TREND
    // =========================

    private int GetTrendScore(Order o) =>
        o.IsTrendAligned switch
        {
            true => 2,
            false => -3,
            null => 0
        };

    // =========================
    // 🔁 CONFIRMATION
    // =========================

    private int GetConfirmationScore(Order o) =>
        o.ConfirmationType switch
        {
            // 🔴 REVERSAL (lo más valioso)
            (int)ConfirmationType.ReversalRetest => 4,
            (int)ConfirmationType.ReversalBreak => 3,

            // 🟡 CONTINUATION (menos edge)
            (int)ConfirmationType.ContinuationRetest => 1,
            (int)ConfirmationType.ContinuationBreak => 0,

            // ⚪ SIN CONFIRMACIÓN
            (int)ConfirmationType.None => -2,
            null => 0,
            _ => 0
        };

    // =========================
    // ⚠️ PIVOT ZONE
    // =========================

    private int GetPivotPenalty(Order o) =>
        o.IsPivotZone == true ? -3 : 0;

    // =========================
    // 🔴 ETAPA 1 CAP
    // =========================

    private int ApplyStageOneCap(int stageId, int score)
    {
        if (stageId == 1 && score >= 3)
            return 2;

        return score;
    }

    // =========================
    // 🎯 CLASIFICACIÓN
    // =========================

    private GradeType GetGrade(int stageId, int score)
    {
        if (stageId == 1)
        {
            return score switch
            {
                >= 2 => GradeType.B,
                1 => GradeType.BC,
                0 => GradeType.CB,
                _ => GradeType.C
            };
        }

        return score switch
        {
            >= 8 => GradeType.A,
            >= 6 => GradeType.AB,
            >= 4 => GradeType.BA,
            >= 2 => GradeType.B,
            >= 0 => GradeType.BC,
            >= -2 => GradeType.CB,
            _ => GradeType.C
        };
    }
}
