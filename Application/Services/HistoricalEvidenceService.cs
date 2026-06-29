using Application.Common;
using Application.DTOs;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class HistoricalEvidenceService : IHistoricalEvidenceService
{
    private const int DefaultMinTrades = 10;
    private const int EvidencePageSize = 10;

    private readonly ICatConvergenceService _catConvergenceService;
    private readonly int _minTrades;

    public HistoricalEvidenceService(
        ICatConvergenceService catConvergenceService,
        IOptions<AiTradeValidationOptions> options)
    {
        _catConvergenceService = catConvergenceService;

        var configuredMinTrades = options.Value.MinHistoricalEvidenceTrades;
        _minTrades = configuredMinTrades > 0 ? configuredMinTrades : DefaultMinTrades;
    }

    public async Task<HistoricalEvidenceDto?> GetEvidenceAsync(
        NormalizedTradeSetupDto setup,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(setup);

        cancellationToken.ThrowIfCancellationRequested();

        var parameters = BuildConvergenceParameters(setup, _minTrades);
        var (data, _) = await _catConvergenceService.GetTBAnalyticsConvergenceAsync(parameters);

        cancellationToken.ThrowIfCancellationRequested();

        var evidence = SelectPrimaryEvidence(data);
        if (evidence is null)
        {
            return null;
        }

        return new HistoricalEvidenceDto
        {
            Setup = evidence.Setup,
            Trades = evidence.Trades,
            TP1Rate = evidence.TP1Rate,
            TP2Rate = evidence.TP2Rate,
            TP3Rate = evidence.TP3Rate,
            SLRate = evidence.SLRate,
            Score = evidence.Score,
            IsSampleSmall = evidence.Trades < _minTrades,
            MinTrades = _minTrades
        };
    }

    private static GetTBAnalyticsConvergenceDto? SelectPrimaryEvidence(
        IEnumerable<GetTBAnalyticsConvergenceDto> evidence)
    {
        return evidence
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Trades)
            .FirstOrDefault();
    }

    private static ParametersTBAnalyticsConvergenceDto BuildConvergenceParameters(
        NormalizedTradeSetupDto setup,
        int minTrades)
    {
        var hasDirection = setup.DirectionId > 0;

        return new ParametersTBAnalyticsConvergenceDto
        {
            InstrumentId = setup.InstrumentId > 0 ? setup.InstrumentId : null,
            TriggerId = setup.TriggerId,
            SceneryId = setup.SceneryId,
            DirectionId = hasDirection ? setup.DirectionId : null,
            FrameId = setup.FrameId,
            FigureId = setup.FigureId,
            TriggerActive = setup.TriggerId.HasValue,
            SceneryActive = setup.SceneryId.HasValue,
            DirectionActive = hasDirection,
            FrameActive = setup.FrameId.HasValue,
            FigureActive = setup.FigureId.HasValue,
            MinTrades = minTrades,
            Skip = 0,
            Take = EvidencePageSize
        };
    }
}
