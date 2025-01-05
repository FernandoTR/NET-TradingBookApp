using Infrastructure;

namespace Application.Interfaces;

public interface IMenuRepository
{
    Task<IEnumerable<Menu>> GetAllAsync();
}
