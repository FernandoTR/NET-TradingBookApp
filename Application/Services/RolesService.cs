using Application.DTOs;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _repository;

    public RolesService(IRolesRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> AddAsync(AspNetRole role, List<AccessMenu> accessMenus)
    {
        return await _repository.AddAsync(role, accessMenus);
    }

    public async Task<AspNetRole?> FindAsync(string id)
    {
        return await _repository.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(AspNetRole role, List<AccessMenu> accessMenus)
    {
        return await _repository.UpdateAsync(role, accessMenus);
    }

    public async Task<IEnumerable<AspNetRole>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<List<string>> GetAllByUserAsync(string userId)
    {
        return await _repository.GetAllByUserAsync(userId);
    }

    public async Task<List<int>> GetAllPermissionsByUserAsync(string userId)
    {
        return await _repository.GetAllPermissionsByUserAsync(userId);
    }

    public async Task<List<GetMenuByUserIdDto>> GetMenuOptionsByUserAsync(string userId)
    {
        return await _repository.GetMenuOptionsByUserAsync(userId);
    }

    public async Task<IEnumerable<AspNetRole>> GetAllRolesByUserAsync(string userId)
    {
        return await _repository.GetAllRolesByUserAsync(userId);
    }
}
