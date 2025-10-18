// Services/IAccountService.cs

using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;

public interface IAccountService
{
  Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
  Task<(bool Succeeded, string Token)> LoginUserAsync(LoginDto loginDto);
  Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
  Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
}