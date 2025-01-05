using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }


    public async Task<CurrentUserDto?> GetCurrentUserInformation(CurrentUserDto user)
    {
        return await _repository.GetCurrentUserInformation(user);
    }

    public async Task<AspNetUser> FindByEmployeeAsync(int id)
    {
        return await _repository.FindByEmployeeAsync(id);
    }

    public async Task<AspNetUser> FindByEmailAsync(string email)
    {
        return await _repository.FindByEmailAsync(email);
    }

    public async Task<AspNetUser> FindAsync(int id)
    {
        return await _repository.FindAsync(id);
    }

    public async Task<AspNetUser> FindAsync(string id)
    {
        return await _repository.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(AspNetUser user)
    {
        return await _repository.UpdateAsync(user);
    }

    public bool VerifyPasswordHash(string passwordHash, string password)
    {
        return _repository.VerifyPasswordHash(passwordHash, password);
    }

    public async Task<bool> ValidatePasswordHistoryAsync(string userId, string newPassword, int historyQuantity)
    {
        return await _repository.ValidatePasswordHistoryAsync(userId, newPassword, historyQuantity);
    }

    public async Task<bool> SavePasswordHistoryAsync(string userId, string passwordHash)
    {
        return await _repository.SavePasswordHistoryAsync(userId, passwordHash);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> IsUserAsync(string userId)
    {
        return await _repository.IsUserAsync(userId);
    }

    public async Task<List<AspNetUser>> UsersWithPermissionAsync(Permissions permision)
    {
        return await _repository.UsersWithPermissionAsync(permision);
    }

    public async Task<AspNetUser> FindUserRolesAsync(string userId)
    {
        return await _repository.FindUserRolesAsync(userId);
    }

    public async Task<bool> UpdateAsync(AspNetUser user, List<AspNetUserRole> listPermissionsMenu)
    {
        return await _repository.UpdateAsync(user, listPermissionsMenu);
    }
}
