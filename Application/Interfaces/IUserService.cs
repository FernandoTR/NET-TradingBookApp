using Application.DTOs;
using Domain.Enums;
using Infrastructure;

namespace Application.Interfaces;

public interface IUserService
{
    Task<CurrentUserDto?> GetCurrentUserInformation(CurrentUserDto user);
    Task<AspNetUser> FindByEmployeeAsync(int id);
    Task<AspNetUser> FindByEmailAsync(string email);
    Task<AspNetUser> FindAsync(int id);
    Task<AspNetUser> FindAsync(string id);
    Task<bool> UpdateAsync(AspNetUser user);
    bool VerifyPasswordHash(string passwordHash, string password);
    Task<bool> ValidatePasswordHistoryAsync(string userId, string newPassword, int historyQuantity);
    Task<bool> SavePasswordHistoryAsync(string userId, string passwordHash);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsUserAsync(string userId);
    Task<List<AspNetUser>> UsersWithPermissionAsync(Permissions permision);
    Task<AspNetUser> FindUserRolesAsync(string userId);
    Task<bool> UpdateAsync(AspNetUser user, List<AspNetUserRole> listPermissionsMenu);
}
