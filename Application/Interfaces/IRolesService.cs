using Application.DTOs;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IRolesService
{
    Task<bool> AddAsync(AspNetRole role, List<AccessMenu> accessMenus);
    Task<AspNetRole?> FindAsync(string id);
    Task<bool> UpdateAsync(AspNetRole role, List<AccessMenu> accessMenus);
    Task<IEnumerable<AspNetRole>> GetAllAsync();
    Task<List<string>> GetAllByUserAsync(string userId);
    Task<List<int>> GetAllPermissionsByUserAsync(string userId);
    Task<List<GetMenuByUserIdDto>> GetMenuOptionsByUserAsync(string userId);
}
