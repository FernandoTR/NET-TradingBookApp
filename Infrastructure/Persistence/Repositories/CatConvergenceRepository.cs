using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Persistence.Repositories;

public class CatConvergenceRepository : ICatConvergenceRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public CatConvergenceRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<(List<GetTBAnalyticsConvergenceDto> data, int totalCount)> GetTBAnalyticsConvergenceAsync(ParametersTBAnalyticsConvergenceDto parameters)
    {
        try
        {
            var sqlParameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = parameters.CategoryId },
                new SqlParameter("@AccountTypeId", SqlDbType.Int) { Value = parameters.AccountTypeId },
                new SqlParameter("@InstrumentId", SqlDbType.Int) { Value = parameters.InstrumentId },
                new SqlParameter("@TriggerId", SqlDbType.Int) { Value = parameters.TriggerId ?? (object)DBNull.Value },
                new SqlParameter("@SceneryId", SqlDbType.Int) { Value = parameters.SceneryId ?? (object)DBNull.Value },
                new SqlParameter("@DirectionId", SqlDbType.Int) { Value = parameters.DirectionId ?? (object)DBNull.Value },
                new SqlParameter("@FrameId", SqlDbType.Int) { Value = parameters.FrameId ?? (object)DBNull.Value },
                new SqlParameter("@FigureId", SqlDbType.Int) { Value = parameters.FigureId ?? (object)DBNull.Value },
                new SqlParameter("@TriggerActive", SqlDbType.Bit) { Value = parameters.TriggerActive },
                new SqlParameter("@SceneryActive", SqlDbType.Bit) { Value = parameters.SceneryActive },
                new SqlParameter("@DirectionActive", SqlDbType.Bit) { Value = parameters.DirectionActive },
                new SqlParameter("@FrameActive", SqlDbType.Bit) { Value = parameters.FrameActive },
                new SqlParameter("@FigureActive", SqlDbType.Bit) { Value = parameters.FigureActive },
                new SqlParameter("@MinTrades", SqlDbType.Int) { Value = parameters.MinTrades ?? 10 },
                new SqlParameter("@SearchValue", SqlDbType.NVarChar) { Value = parameters.SearchValue ?? (object)DBNull.Value },
                new SqlParameter("@OrderByColumn", SqlDbType.NVarChar) { Value = parameters.OrderByColumn ?? (object)DBNull.Value },
                new SqlParameter("@SortColumnDir", SqlDbType.NVarChar) { Value = parameters.SortColumnDir ?? (object)DBNull.Value },
                new SqlParameter("@Skip", SqlDbType.Int) { Value = parameters.Skip },
                new SqlParameter("@Take", SqlDbType.Int) { Value = parameters.Take },
                new SqlParameter("@Count", SqlDbType.Int) { Direction = ParameterDirection.Output }
            };

            var result = await _context.Set<GetTBAnalyticsConvergenceDto>()
                .FromSqlRaw("EXEC usp_GetTBAnalyticsConvergence @CategoryId, @AccountTypeId, @InstrumentId, @TriggerId, @SceneryId, @DirectionId, @FrameId, @FigureId, @TriggerActive, @SceneryActive, @DirectionActive, @FrameActive, @FigureActive, @MinTrades, @SearchValue, @OrderByColumn, @SortColumnDir, @Skip, @Take, @Count OUTPUT", sqlParameters)
                .ToListAsync();

            var totalCount = (int)sqlParameters.Last().Value;

            return (result, totalCount);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetTBAnalyticsConvergenceAsync), ex);
            return (new List<GetTBAnalyticsConvergenceDto>(), 0);
        }
    }
}
