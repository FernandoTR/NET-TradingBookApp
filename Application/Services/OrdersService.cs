using Application.Common;
using Application.DTOs;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Models;
using Infrastructure;

namespace Application.Services;

public class OrdersService : IOrdersService
{
    private readonly IGenericRepository<Order> _repository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly ICatTimeRepository _catTimeRepository;
    private readonly ITradesService _tradesService;
    private readonly ILogService _logService;
    private readonly IAccountBalancesService _accountBalancesService;
    private readonly IAccountsService _accountsService;
    private readonly ITradingScoreEngineService _tradingScoreEngineService;
    private readonly ITradingViewDownloaderServices _tradingViewDownloaderServices;
    private readonly IAiTradeValidationMetricService _aiTradeValidationMetricService;
    private readonly IContextualScoreInputAdapter? _contextualScoreInputAdapter;

    public OrdersService(IGenericRepository<Order> repository,
                         IOrdersRepository ordersRepository,
                         ICatTimeRepository catTimeRepository,
                         ITradesService tradesService,
                         ILogService logService,
                         IAccountBalancesService accountBalancesService,
                         IAccountsService accountsService,
                         ITradingScoreEngineService tradingScoreEngineService,
                         ITradingViewDownloaderServices tradingViewDownloaderServices,
                         IAiTradeValidationMetricService aiTradeValidationMetricService,
                         IContextualScoreInputAdapter? contextualScoreInputAdapter = null)
    {
        _repository = repository;
        _ordersRepository = ordersRepository;
        _catTimeRepository = catTimeRepository;
        _tradesService = tradesService;
        _logService = logService;
        _accountBalancesService = accountBalancesService;
        _accountsService = accountsService;
        _tradingScoreEngineService = tradingScoreEngineService;
        _tradingViewDownloaderServices = tradingViewDownloaderServices;
        _aiTradeValidationMetricService = aiTradeValidationMetricService;
        _contextualScoreInputAdapter = contextualScoreInputAdapter;
    }

    public async Task<bool> AddAsync(Order entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(Order entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var existing = await _repository.GetByIdAsync(entity.Id);
        if (existing is null) return false;
        ApplyRecordedChanges(existing, entity);
        var updated = await _repository.UpdateAsync(existing);
        if (updated)
        {
            await _aiTradeValidationMetricService.RefreshOrderOutcomeAsync(entity.Id, CancellationToken.None);
        }

        return updated;
    }

    public async Task<(List<GetOrdersDataTableDto>, int count)> GetOrdersDataTableAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _ordersRepository.GetOrdersDataTableAsync(parameters);
    }

    public async Task<List<GetTBAnalyticsTimeDto>> GetTBAnalyticsTimeAsync(ParametersTBAnalyticsDto parameters)
    {
        return await _catTimeRepository.GetTBAnalyticsTimeAsync(parameters);
    }

    public async Task<(bool, int)> AddOrderAsync(Order entity, Trade trade)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(trade);
        if (entity.StructuralScore is null && entity.TotalScore is null && entity.Grade is null)
            _tradingScoreEngineService.Evaluate(entity);

        return await _ordersRepository.AddOrderAsync(entity, trade);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        // Recuperar el trade asociado a la orden
        var options = new QueryOptions<Trade>
        {
            Where = x => x.OrderId == id
        };

        var trade = (await _tradesService.GetAllAsync(options)).FirstOrDefault();
        if (trade == null)
        {
            return false;
        }

        var resultTrade = await _tradesService.DeleteAsync(trade.Id);
        if (!resultTrade)
        {
            return false;
        }

        // Recuperar de la tabla AccountBalances la acción que se realizó en la orden
        var accountBalance = await _accountBalancesService.GetByOrderIdAsync(id);
        if (accountBalance != null)
        {
            var balance = new AccountBalance
            {
                AccountId = accountBalance.AccountId,
                OrderId = null,
                Balance = Math.Abs(accountBalance.Balance),
                Reference = $"Cancelación de orden #{id}, {accountBalance.Reference}"
            };


            await _accountBalancesService.DeleteAsync(accountBalance.Id);

            // Descuenta el monto de la cuenta seleccionada
            await _accountsService.AddCashAsync(balance);
        }


        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> CloseOrderAsync(Order entity, Trade trade)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(trade);
        var existing = await _repository.GetByIdAsync(entity.Id);
        if (existing is null) return false;
        ApplyRecordedChanges(existing, entity);
        var closed = await _ordersRepository.CloseOrderAsync(existing, trade);
        if (closed)
        {
            await _aiTradeValidationMetricService.RefreshOrderOutcomeAsync(entity.Id, CancellationToken.None);
        }

        return closed;
    }

    /// <inheritdoc />
    public OrderContextualScoreEvaluationDto ReevaluateContext(Order entity, ContextualScoreInputDto input)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(input);
        var original = new LegacyTradingScoreDto(entity.StructuralScore, entity.TotalScore, entity.Grade);
        var evaluation = _tradingScoreEngineService.Evaluate(input);
        return new(entity.Id > 0 ? entity.Id : null, entity.CatSceneryId, original, evaluation);
    }

    /// <inheritdoc />
    public async Task<OrderContextualScoreEvaluationDto> ReevaluateContextAsync(Order entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();
        var adapter = _contextualScoreInputAdapter ?? throw new InvalidOperationException("Contextual input adaptation must be configured.");
        var input = await adapter.FromOrderAsync(entity, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return ReevaluateContext(entity, input);
    }

    // Merge onto the tracked row: detached MVC submissions omit legacy score fields.
    // Context edits and operational updates retain the original evaluation; recalculation has its own explicit API.
    private static void ApplyRecordedChanges(Order target, Order changes)
    {
        target.AlterDate = changes.AlterDate;
        target.AuthorId = changes.AuthorId;
        target.CatCategoryId = changes.CatCategoryId;
        target.AccountId = changes.AccountId;
        target.CatInstrumentId = changes.CatInstrumentId;
        target.Date = changes.Date;
        target.Time = changes.Time;
        target.CatDayId = changes.CatDayId;
        target.CatStageId = changes.CatStageId;
        target.CatFigureId = changes.CatFigureId;
        target.CatFrameId = changes.CatFrameId;
        target.CatTriggerId = changes.CatTriggerId;
        target.CatDirectionId = changes.CatDirectionId;
        target.CatSceneryId = changes.CatSceneryId;
        target.CatStatusId = changes.CatStatusId;
        target.IsTrendAligned = changes.IsTrendAligned;
        target.LocationType = changes.LocationType;
        target.ConfirmationType = changes.ConfirmationType;
        target.IsPivotZone = changes.IsPivotZone;
        target.Sl = changes.Sl;
        target.Tp1 = changes.Tp1;
        target.Tp2 = changes.Tp2;
        target.Tp3 = changes.Tp3;
        target.Target = changes.Target;
        target.Chart = changes.Chart;
        target.Comments = changes.Comments;
    }

    public async Task<bool> DownloadImageTradingViewAsync()
    {
        var result = await _repository.GetAllAsync();
        var dataset = result.Where(z => z.Id > 116).SelectMany(x=> new List<(int, string)> {(x.Id,x.Chart) }).ToList();

        _tradingViewDownloaderServices.DescargarImagenes(dataset);

        return true;
    }

}
