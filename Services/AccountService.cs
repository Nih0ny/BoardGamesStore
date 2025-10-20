// Services/AccountService.cs

using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using BoardGamesStore.Services.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web; // Needed for HttpUtility.UrlEncode

public class AccountService : IAccountService
{
  private readonly UserManager<User> _userManager; // Assume your model is named User
  private readonly SignInManager<User> _signInManager;
  private readonly IEmailService _emailService;
  private readonly IPasswordHasher<User> _hasherService;
  private readonly ITokenService _tokenService;
  private readonly ApplicationDbContext _context;

  public AccountService(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEmailService emailService,
      IPasswordHasher<User> passwordHasher,
      ITokenService tokenService,
      ApplicationDbContext context
    )
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _emailService = emailService;
    _hasherService = passwordHasher;
    _tokenService = tokenService;
    _context = context;
  }

  public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
  {
    var user = new User { UserName = registerDto.Name, Email = registerDto.Email };
    var result = await _userManager.CreateAsync(user, _hasherService.HashPassword(user, registerDto.Password));

    if (result.Succeeded)
    {
      // Generate token for email confirmation
      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      // Encode the token so it is safe for URLs
      var encodedToken = HttpUtility.UrlEncode(token);

      // Build the link (replace "https://yourapi.com" with your real address)
      var confirmationLink = $"http://localhost:5177/api/account/confirm-email?userId={user.Id}&token={encodedToken}";

      // Send the email
      await _emailService.SendEmailAsync(
          user.Email,
          "Confirm your registration",
          $"Please confirm your registration by clicking the following link: <a href='{confirmationLink}'>link</a>");
    }

    return result;
  }

  public async Task<(string AccessToken, string RefreshToken)> LoginUserAsync(LoginDto loginDto)
  {
    var user = await _userManager.FindByEmailAsync(loginDto.Email);
    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
    {
      throw new UnauthorizedAccessException("Invalid login credentials.");
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);

    if (result.Succeeded)
    {
      var accessToken = await _tokenService.GenerateJwtTokenAsync(user);
      var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);
      return (accessToken, refreshToken);
    }

    throw new UnauthorizedAccessException("Invalid login attempt.");
  }

  public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
  {
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found." });
    }

    // ASP.NET Identity automatically handles token decoding
    return await _userManager.ConfirmEmailAsync(user, token);
  }

  public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
  {
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found." });
    }

    return await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
  }

  public async Task<IdentityResult> ForgotPasswordAsync(string email)
  {
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found or email not confirmed." });
    }

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var encodedToken = HttpUtility.UrlEncode(token);

    var resetLink = $"http://localhost:5177/reset-password?userId={user.Id}&token={encodedToken}";

    await _emailService.SendEmailAsync(
        email,
        "Password Reset",
        $"You can reset your password by clicking the following link: <a href='{resetLink}'>link</a>");

    return IdentityResult.Success;
  }

  public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
  {
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found." });
    }

    return await _userManager.ResetPasswordAsync(user, token, newPassword);
  }
}