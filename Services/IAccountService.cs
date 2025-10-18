// Services/IAccountService.cs

using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;

public interface IAccountService
{
  Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
  Task<(bool Succeeded, string Token)> LoginUserAsync(LoginDto loginDto);
  Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
  Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
  Task<IdentityResult> ForgotPasswordAsync(string email);
  Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
}