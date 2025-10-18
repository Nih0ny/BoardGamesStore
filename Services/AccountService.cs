// Services/AccountService.cs

using BoardGamesStore.Models;
using BoardGamesStore.Services.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
  private readonly JwtSettings _jwtSettings;

  public AccountService(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEmailService emailService,
      IPasswordHasher<User> passwordHasher,
      IOptions<JwtSettings> jwtSettings
    )
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _emailService = emailService;
    _hasherService = passwordHasher;
    _jwtSettings = jwtSettings.Value;
  }

  public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
  {
    var user = new User { UserName = registerDto.Name, Name = registerDto.Name, Email = registerDto.Email, Password = "" };
    var password = _hasherService.HashPassword(user, registerDto.Password);
    var result = await _userManager.CreateAsync(user, password);

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

  public async Task<(bool Succeeded, string Token)> LoginUserAsync(LoginDto loginDto)
  {
    var user = await _userManager.FindByEmailAsync(loginDto.Email);
    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
    {
      return (false, string.Empty);
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);

    if (result.Succeeded)
    {
      var roles = await _userManager.GetRolesAsync(user);
      var userName = user.UserName ?? user.Name ?? user.Email ?? "Unknown";
      var token = GenerateAccessToken(user.Id, userName, roles);
      return (true, token);
    }

    return (false, string.Empty);
  }

  private string GenerateAccessToken(int userId, string userName, IEnumerable<string> roles)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, userId.ToString()),
      new(JwtRegisteredClaimNames.Name, userName),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };

    if (roles != null)
    {
      foreach (var role in roles)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
      }
    }

    var token = new JwtSecurityToken(
      issuer: _jwtSettings.Issuer,
      audience: _jwtSettings.Audience,
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
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