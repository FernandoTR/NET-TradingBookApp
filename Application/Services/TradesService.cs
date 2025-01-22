using Application.Common;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class TradesService : ITradesService
{
    private readonly IGenericRepository<Trade> _repository;
    public TradesService(IGenericRepository<Trade> repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(Trade entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Trade>> GetAllAsync(QueryOptions<Trade>? options = null)
    {
        return await _repository.GetAllAsync(options);
    }

    public async Task<Trade?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(Trade entity)
    {
        return await _repository.UpdateAsync(entity);
    }
}
