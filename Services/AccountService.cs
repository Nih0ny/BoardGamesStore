// Services/AccountService.cs

using BoardGamesStore.Models;
using BoardGamesStore.Services.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web; // Потрібно для HttpUtility.UrlEncode

public class AccountService : IAccountService
{
  private readonly UserManager<User> _userManager; // Припустимо, ваша модель називається User
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
      // Генеруємо токен для підтвердження пошти
      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      // Кодуємо токен, щоб він був безпечним для URL
      var encodedToken = HttpUtility.UrlEncode(token);

      // Формуємо посилання (замініть "https://yourapi.com" на вашу реальну адресу)
      var confirmationLink = $"https://yourapi.com/api/account/confirm-email?userId={user.Id}&token={encodedToken}";

      // Відправляємо лист
      await _emailService.SendEmailAsync(
          user.Email,
          "Підтвердіть вашу реєстрацію",
          $"Будь ласка, підтвердіть вашу реєстрацію, перейшовши за посиланням: <a href='{confirmationLink}'>link</a>");
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
      return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено." });
    }

    // ASP.NET Identity автоматично обробляє декодування токену
    return await _userManager.ConfirmEmailAsync(user, token);
  }

  public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
  {
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено." });
    }

    return await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
  }
}