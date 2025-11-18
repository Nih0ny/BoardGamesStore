using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.AspNetCore.Identity;
using System.Web;

public class AccountService : IAccountService
{
  private readonly UserManager<User> _userManager;
  private readonly SignInManager<User> _signInManager;
  private readonly IEmailService _emailService;
  private readonly ITokenService _tokenService;

  public AccountService(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEmailService emailService,
      ITokenService tokenService
    )
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _emailService = emailService;
    _tokenService = tokenService;
  }

  public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
  {
    var user = await _userManager.FindByEmailAsync(registerDto.Email);
    if (user != null && await _userManager.IsEmailConfirmedAsync(user))
    {
      return IdentityResult.Failed(new IdentityError { Description = "Email is already registered." });
    }

    if (user == null)
    {
      user = new User { UserName = registerDto.Name, Email = registerDto.Email };
      if (!(await _userManager.CreateAsync(user, registerDto.Password)).Succeeded)
      {
        return IdentityResult.Failed(new IdentityError { Description = "User registration failed." });
      }
      else
      {
        await _userManager.AddToRoleAsync(user, "User");
      }
    }

    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    var encodedToken = HttpUtility.UrlEncode(token);
    var confirmationLink = $"{registerDto.ClientConfirmationUrl}?email={user.Email}&token={encodedToken}";
    await _emailService.SendEmailAsync(
        user.Email!,
        "Confirm your registration",
        $"Please confirm your registration by clicking the following link: <a href='{confirmationLink}'>link</a>");

    return IdentityResult.Success;
  }

  public async Task<(string AccessToken, string RefreshToken)> LoginUserAsync(LoginDto loginDto)
  {
    var user = await _userManager.FindByEmailAsync(loginDto.Email);
    if (user == null || /*!await _userManager.IsEmailConfirmedAsync(user)*/false) // FIXME: тимчасово вимкнено підтвердження email
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

  public async Task<IdentityResult> ConfirmEmailAsync(string email, string token)
  {
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found." });
    }

    return await _userManager.ConfirmEmailAsync(user, token);
  }

  public async Task<IdentityResult> ChangePasswordAsync(string email, ChangePasswordDto changePasswordDto)
  {
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found." });
    }

    return await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
  }

  public async Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
  {
    var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found or email not confirmed." });
    }

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var encodedToken = HttpUtility.UrlEncode(token);

    var resetLink = $"{forgotPasswordDto.ClientResetPasswordUrl}?email={user.Email}&token={encodedToken}";

    await _emailService.SendEmailAsync(
        user.Email!,
        "Password Reset",
        $"You can reset your password by clicking the following link: <a href='{resetLink}'>link</a>");

    return IdentityResult.Success;
  }

  public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
  {
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "User not found." });
    }

    return await _userManager.ResetPasswordAsync(user, token, newPassword);
  }
}