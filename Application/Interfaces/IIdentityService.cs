using Application.DTOs;
using Application.Models;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<string?> GetUserEmailAsync(string userId);
    Task<CurrentUserDto?> GetUserByNameAsync(string userName);  
    Task<bool> IsEmailConfirmedAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password, int employeeId);
    Task<Result> DeleteUserAsync(string userId);
    Task<SignInResult> PasswordSignInAsync(string userName, string password, bool rememberMe);
    Task SignOutAsync();    
    CurrentUserDto? GetCurrentUserAsync();
    bool IsUserAuthenticated();
    string GetCurrentUserId();
    Task<string> GeneratePasswordResetTokenAsync(string userId);
    Task<IdentityResult> ResetPasswordAsync(string userId, string token, string password);
    Task<IList<string>> GetValidTwoFactorProvidersAsync();
    Task<Result> SendTwoFactorCodeAsync(string userId, string provider);
    Task<Result> SendTwoFactorCodeAsync(string provider);
    Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser);


}

