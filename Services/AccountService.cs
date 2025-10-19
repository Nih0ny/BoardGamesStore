// Services/AccountService.cs

using BoardGamesStore.Models;
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
  private readonly JwtSettings _jwtSettings;

  private readonly IDistributedCache _cache;
  public AccountService(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      IEmailService emailService,
      IPasswordHasher<User> passwordHasher,
      IOptions<JwtSettings> jwtSettings,
      IDistributedCache cache
    )
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _emailService = emailService;
    _hasherService = passwordHasher;
    _jwtSettings = jwtSettings.Value;
    _cache = cache;
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
      // var token = GenerateAccessToken(user.Id, userName, roles);
      // return (true, token);
    }

    return (false, string.Empty);
  }

  // public async Task<(bool Succeeded, string AccessToken, string RefreshToken)> RefreshToken(string refreshToken)
  // {
  //   // logic for update tokens
  //   var (succeeded, newAccessToken, newRefreshToken) = await RefreshTokensAsync(refreshToken);
  //   if (succeeded)
  //   {
  //     return (true, newAccessToken, newRefreshToken);
  //   }

  //   return (false, null, null);
  // }

  public async Task<(bool Succeeded, string? AccessToken, string? RefreshToken)> RefreshTokensAsync(string expiredAccessToken, string refreshToken)
  {
    // 1. Валідуємо принципала зі старого access токена (навіть якщо він прострочений)
    var principal = GetPrincipalFromExpiredToken(refreshToken, _jwtSettings.RefreshSecret);
    if (principal?.Identity?.Name is null)
    {
      return (false, null, null); // Невалідований токен
    }

    // 2. Знаходимо користувача за іменем (або ID)
    var user = await _userManager.FindByNameAsync(principal.Identity.Name);
    // if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
    // {
    //   // Користувач не знайдений, токен не співпадає або прострочений
    //   return (false, null, null);
    // }

    // 3. Генеруємо нову пару токенів
    var newAccessToken = GenerateToken(
        user.Id,
        user.UserName ?? user.Name ?? user.Email ?? "Unknown",
        await _userManager.GetRolesAsync(user),
        _jwtSettings.AccessSecret,
        _jwtSettings.AccessAudience,
        DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
    );
    var newRefreshToken = GenerateToken(
        user.Id,
        user.UserName ?? user.Name ?? user.Email ?? "Unknown",
        await _userManager.GetRolesAsync(user),
        _jwtSettings.RefreshSecret,
        _jwtSettings.RefreshAudience,
        DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
    );

    await AddRefreshTokenToCache(user.Id.ToString(), newRefreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

    await _userManager.UpdateAsync(user);

    return (true, newAccessToken, newRefreshToken);
  }

  private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, string secret)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = false, // Ви можете перевіряти, якщо потрібно
      ValidateIssuer = false,   // Ви можете перевіряти, якщо потрібно
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
      ValidateLifetime = false // <-- ВАЖЛИВО: не перевіряємо час життя, бо токен вже прострочений
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    SecurityToken securityToken;

    try
    {
      var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
      var jwtSecurityToken = securityToken as JwtSecurityToken;

      // Перевіряємо, що алгоритм підпису той, який ми очікуємо
      if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
      {
        return null;
      }

      return principal;
    }
    catch (Exception)
    {
      // Якщо токен взагалі невалідований (наприклад, підроблений підпис)
      return null;
    }
  }

  private string GenerateToken(int userId, string userName, IEnumerable<string> roles, string secret, string audience, DateTime expires)
  {
    var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);

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
      audience: audience,
      claims: claims,
      expires: expires,
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  // create function for adding refresh token in cache
  private async Task AddRefreshTokenToCache(string userId, string refreshToken, DateTime expiration)
  {
    // Serialize using Newtonsoft.Json (JsonConvert) and build a cache key per user
    string jsonReport = JsonConvert.SerializeObject(refreshToken);

    var options = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(expiration); // Час життя кешу

    // Зберігаємо в кеш Valkey
    var cacheKey = $"refresh_token_{userId}";
    await _cache.SetStringAsync(cacheKey, jsonReport, options);
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