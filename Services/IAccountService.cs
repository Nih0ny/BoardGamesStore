// Services/IAccountService.cs

using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;

public interface IAccountService
{
  Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
  Task<(string AccessToken, string RefreshToken)> LoginUserAsync(LoginDto loginDto);
  Task<IdentityResult> ConfirmEmailAsync(string email, string token);
  Task<IdentityResult> ChangePasswordAsync(string email, ChangePasswordDto changePasswordDto);
  Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
  Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
}