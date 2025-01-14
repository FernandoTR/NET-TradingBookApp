using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class CatAccountTypeService : ICatAccountTypeService
{
    private readonly IGenericRepository<CatAccountType> _repository;
    public CatAccountTypeService(IGenericRepository<CatAccountType> repository)
    {
        _repository = repository;
    }

    public Task<bool> AddAsync(CatAccountType entity)
    {
        return _repository.AddAsync(entity);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<CatAccountType>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<CatAccountType?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(CatAccountType entity)
    {
        return _repository.UpdateAsync(entity);
    }
}
