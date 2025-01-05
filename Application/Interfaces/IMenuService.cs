using Infrastructure;

namespace Application.Interfaces;

public interface IMenuService
{
    Task<IEnumerable<Menu>> GetAllAsync();
}
