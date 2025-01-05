using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _repository;

    public MenuService(IMenuRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Menu>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

}
