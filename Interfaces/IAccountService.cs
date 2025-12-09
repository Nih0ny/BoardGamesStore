using BoardGamesStore.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Interfaces;

public interface IAccountService
{
  Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
  Task<Result<(string AccessToken, string RefreshToken)>> LoginUserAsync(LoginDto loginDto);
  Task<IdentityResult> ConfirmEmailAsync(string email, string token);
  Task<IdentityResult> ChangePasswordAsync(string email, ChangePasswordDto changePasswordDto);
  Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
  Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
}