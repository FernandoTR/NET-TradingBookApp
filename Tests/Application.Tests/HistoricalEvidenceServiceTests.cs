using Application.Common;
using Application.DTOs;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Options;

namespace Application.Tests;

public class HistoricalEvidenceServiceTests
{
    [Fact]
    public async Task GetEvidenceAsync_MapsNormalizedSetupToConvergenceParameters()
    {
        var convergenceService = new FakeCatConvergenceService(
            new GetTBAnalyticsConvergenceDto { Setup = "setup", Trades = 12, Score = 80m });
        var service = CreateService(convergenceService, minTrades: 15);

        await service.GetEvidenceAsync(new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 2,
            TriggerId = 3,
            SceneryId = 4,
            FrameId = 5,
            FigureId = 6
        }, CancellationToken.None);

        var parameters = convergenceService.LastParameters;

        Assert.NotNull(parameters);
        Assert.Equal(1, parameters.InstrumentId);
        Assert.Equal(2, parameters.DirectionId);
        Assert.Equal(3, parameters.TriggerId);
        Assert.Equal(4, parameters.SceneryId);
        Assert.Equal(5, parameters.FrameId);
        Assert.Equal(6, parameters.FigureId);
        Assert.True(parameters.TriggerActive);
        Assert.True(parameters.SceneryActive);
        Assert.True(parameters.DirectionActive);
        Assert.True(parameters.FrameActive);
        Assert.True(parameters.FigureActive);
        Assert.Equal(15, parameters.MinTrades);
        Assert.Equal(0, parameters.Skip);
        Assert.Equal(10, parameters.Take);
    }

    [Fact]
    public async Task GetEvidenceAsync_ActivatesOnlyAvailableNormalizedFilters()
    {
        var convergenceService = new FakeCatConvergenceService(
            new GetTBAnalyticsConvergenceDto { Setup = "setup", Trades = 12, Score = 80m });
        var service = CreateService(convergenceService);

        await service.GetEvidenceAsync(new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 2
        }, CancellationToken.None);

        var parameters = convergenceService.LastParameters;

        Assert.NotNull(parameters);
        Assert.True(parameters.DirectionActive);
        Assert.False(parameters.TriggerActive);
        Assert.False(parameters.SceneryActive);
        Assert.False(parameters.FrameActive);
        Assert.False(parameters.FigureActive);
        Assert.Null(parameters.TriggerId);
        Assert.Null(parameters.SceneryId);
        Assert.Null(parameters.FrameId);
        Assert.Null(parameters.FigureId);
    }

    [Fact]
    public async Task GetEvidenceAsync_MarksSampleSmallWhenTradesAreBelowMinimum()
    {
        var convergenceService = new FakeCatConvergenceService(
            new GetTBAnalyticsConvergenceDto { Setup = "small", Trades = 9, Score = 70m });
        var service = CreateService(convergenceService, minTrades: 10);

        var evidence = await service.GetEvidenceAsync(new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 2
        }, CancellationToken.None);

        Assert.NotNull(evidence);
        Assert.True(evidence.IsSampleSmall);
        Assert.Equal(10, evidence.MinTrades);
    }

    [Fact]
    public async Task GetEvidenceAsync_ReturnsNullWhenConvergenceHasNoEvidence()
    {
        var convergenceService = new FakeCatConvergenceService();
        var service = CreateService(convergenceService);

        var evidence = await service.GetEvidenceAsync(new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 2
        }, CancellationToken.None);

        Assert.Null(evidence);
    }

    [Fact]
    public async Task GetEvidenceAsync_SelectsHighestScoreThenHighestTrades()
    {
        var convergenceService = new FakeCatConvergenceService(
            new GetTBAnalyticsConvergenceDto { Setup = "more-trades", Trades = 20, Score = 80m },
            new GetTBAnalyticsConvergenceDto { Setup = "best-score", Trades = 12, Score = 90m },
            new GetTBAnalyticsConvergenceDto { Setup = "best-score-tie", Trades = 18, Score = 90m });
        var service = CreateService(convergenceService);

        var evidence = await service.GetEvidenceAsync(new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 2
        }, CancellationToken.None);

        Assert.NotNull(evidence);
        Assert.Equal("best-score-tie", evidence.Setup);
        Assert.Equal(18, evidence.Trades);
        Assert.Equal(90m, evidence.Score);
    }

    [Fact]
    public async Task GetEvidenceAsync_DelegatesToConvergenceServiceWithoutDynamicSqlInputs()
    {
        var convergenceService = new FakeCatConvergenceService(
            new GetTBAnalyticsConvergenceDto { Setup = "setup", Trades = 12, Score = 80m });
        var service = CreateService(convergenceService);

        await service.GetEvidenceAsync(new NormalizedTradeSetupDto
        {
            InstrumentId = 1,
            DirectionId = 2,
            TriggerId = 3
        }, CancellationToken.None);

        var parameters = convergenceService.LastParameters;

        Assert.Equal(1, convergenceService.CallCount);
        Assert.NotNull(parameters);
        Assert.Null(parameters.SearchValue);
        Assert.Null(parameters.OrderByColumn);
        Assert.Null(parameters.SortColumnDir);
    }

    private static HistoricalEvidenceService CreateService(
        FakeCatConvergenceService convergenceService,
        int minTrades = 10)
    {
        return new HistoricalEvidenceService(
            convergenceService,
            Options.Create(new AiTradeValidationOptions
            {
                MinHistoricalEvidenceTrades = minTrades
            }));
    }
}

internal sealed class FakeCatConvergenceService : ICatConvergenceService
{
    private readonly List<GetTBAnalyticsConvergenceDto> _data;

    public FakeCatConvergenceService(params GetTBAnalyticsConvergenceDto[] data)
    {
        _data = data.ToList();
    }

    public int CallCount { get; private set; }

    public ParametersTBAnalyticsConvergenceDto? LastParameters { get; private set; }

    public Task<(List<GetTBAnalyticsConvergenceDto> data, int totalCount)> GetTBAnalyticsConvergenceAsync(
        ParametersTBAnalyticsConvergenceDto parameters)
    {
        CallCount++;
        LastParameters = parameters;

        return Task.FromResult((_data, _data.Count));
    }
}
