using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BoardGamesStore.Services;

public sealed record RefreshTokenRequest(string Token, string RefreshToken);
public sealed record RefreshTokenResponse(string Token, string RefreshToken);

// public class TokenService : ITokenService
// {
//   // public async Task<(bool Succeeded, string AccessToken, string RefreshToken)> RefreshToken(string refreshToken)
//   // {
//   //   // logic for update tokens
//   //   var (succeeded, newAccessToken, newRefreshToken) = await RefreshTokensAsync(refreshToken);
//   //   if (succeeded)
//   //   {
//   //     return (true, newAccessToken, newRefreshToken);
//   //   }

//   //   return (false, null, null);
//   // }

//   // public async Task<(bool Succeeded, string? AccessToken, string? RefreshToken)> RefreshTokensAsync(string expiredAccessToken, string refreshToken)
//   // {
//   //   // 1. Валідуємо принципала зі старого access токена (навіть якщо він прострочений)
//   //   var principal = GetPrincipalFromExpiredToken(refreshToken, _jwtSettings.RefreshSecret);
//   //   if (principal?.Identity?.Name is null)
//   //   {
//   //     return (false, null, null); // Невалідований токен
//   //   }

//   //   // 2. Знаходимо користувача за іменем (або ID)
//   //   var user = await _userManager.FindByNameAsync(principal.Identity.Name);
//   //   if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
//   //   {
//   //     // Користувач не знайдений, токен не співпадає або прострочений
//   //     return (false, null, null);
//   //   }

//   //   // 3. Генеруємо нову пару токенів
//   //   var newAccessToken = GenerateToken(
//   //       user.Id,
//   //       user.UserName ?? user.Name ?? user.Email ?? "Unknown",
//   //       await _userManager.GetRolesAsync(user),
//   //       _jwtSettings.AccessSecret,
//   //       _jwtSettings.AccessAudience,
//   //       DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
//   //   );
//   //   var newRefreshToken = GenerateToken(
//   //       user.Id,
//   //       user.UserName ?? user.Name ?? user.Email ?? "Unknown",
//   //       await _userManager.GetRolesAsync(user),
//   //       _jwtSettings.RefreshSecret,
//   //       _jwtSettings.RefreshAudience,
//   //       DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
//   //   );

//   //   await AddRefreshTokenToCache(user.Id.ToString(), newRefreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays));

//   //   await _userManager.UpdateAsync(user);

//   //   return (true, newAccessToken, newRefreshToken);
//   // }

//   // private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, string secret)
//   // {
//   //   var tokenValidationParameters = new TokenValidationParameters
//   //   {
//   //     ValidateAudience = false, // Ви можете перевіряти, якщо потрібно
//   //     ValidateIssuer = false,   // Ви можете перевіряти, якщо потрібно
//   //     ValidateIssuerSigningKey = true,
//   //     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
//   //     ValidateLifetime = false // <-- ВАЖЛИВО: не перевіряємо час життя, бо токен вже прострочений
//   //   };

//   //   var tokenHandler = new JwtSecurityTokenHandler();
//   //   SecurityToken securityToken;

//   //   try
//   //   {
//   //     var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
//   //     var jwtSecurityToken = securityToken as JwtSecurityToken;

//   //     // Перевіряємо, що алгоритм підпису той, який ми очікуємо
//   //     if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
//   //     {
//   //       return null;
//   //     }

//   //     return principal;
//   //   }
//   //   catch (Exception)
//   //   {
//   //     // Якщо токен взагалі невалідований (наприклад, підроблений підпис)
//   //     return null;
//   //   }
//   // }

//   // private string GenerateToken(int userId, string userName, IEnumerable<string> roles, string secret, string audience, DateTime expires)
//   // {
//   //   var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);

//   //   var claims = new List<Claim>
//   //   {
//   //     new(JwtRegisteredClaimNames.Sub, userId.ToString()),
//   //     new(JwtRegisteredClaimNames.Name, userName),
//   //     new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//   //     new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
//   //   };

//   //   if (roles != null)
//   //   {
//   //     foreach (var role in roles)
//   //     {
//   //       claims.Add(new Claim(ClaimTypes.Role, role));
//   //     }
//   //   }

//   //   var token = new JwtSecurityToken(
//   //     issuer: _jwtSettings.Issuer,
//   //     audience: audience,
//   //     claims: claims,
//   //     expires: expires,
//   //     signingCredentials: credentials);

//   //   return new JwtSecurityTokenHandler().WriteToken(token);
//   // }

//   // private async Task AddRefreshTokenToCache(string userId, string refreshToken, DateTime expiration)
//   {
//     // Serialize using Newtonsoft.Json (JsonConvert) and build a cache key per user
//     string jsonReport = JsonConvert.SerializeObject(refreshToken);

//   var options = new DistributedCacheEntryOptions()
//       .SetAbsoluteExpiration(expiration); // Час життя кешу

//   // Зберігаємо в кеш Valkey
//   var cacheKey = $"refresh_token_{userId}";
//   private UserManager<User> _userManager;

//   await _cache.SetStringAsync(cacheKey, jsonReport, options);
//   }

//   // public string GenerateAccessToken(string userId, string userEmail)
//   // {
//   //   throw new NotImplementedException();
//   // }

//   TokenService(
//       UserManager<User> userManager,
//       RoleManager<IdentityRole> roleManager,
//       IOptions<AuthenticationOptions> authOptions,
//       TokenValidationParameters tokenValidationParameters,
//       ApplicationDbContext dbContext)
//   {
//     _userManager = userManager;
//     _roleManager = roleManager;
//     _authOptions = authOptions;
//     _tokenValidationParameters = tokenValidationParameters;
//     _dbContext = dbContext;
//   }

//   private async Task<string> GenerateRefreshTokenAsync(string token, User user, string? existingRefreshToken)
//   {
//     var tokenHandler = new JwtSecurityTokenHandler();
//     var jwtToken = tokenHandler.ReadJwtToken(token);
//     var jti = jwtToken.Id;

//     var refreshToken = new RefreshToken
//     {
//       Token = Guid.NewGuid().ToString(),
//       JwtId = jti,
//       UserId = user.Id,
//       ExpiryDate = DateTime.UtcNow.AddDays(7),
//       CreatedAtUtc = DateTime.UtcNow,
//     };

//     if (!string.IsNullOrEmpty(existingRefreshToken))
//     {
//       var existingToken = await _dbContext.RefreshTokens
//           .FirstOrDefaultAsync(x => x.Token == existingRefreshToken);

//       if (existingToken != null)
//       {
//         _dbContext.Set<RefreshToken>().Remove(existingToken);
//       }
//     }

//     await _dbContext.AddAsync(refreshToken);
//     await _dbContext.SaveChangesAsync();

//     return refreshToken.Token;
//   }

//   public async Task<RefreshTokenResponse> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
//   {
//     // Валідація цифрового підпису наданого токена доступу
//     var validatedToken = GetPrincipalFromToken(token, _tokenValidationParameters);
//     if (validatedToken is null)
//     {
//       throw new SecurityException("Invalid token");
//     }

//     // Отримання JWT id з клеймів
//     var jti = validatedToken.Claims
//         .SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

//     if (string.IsNullOrEmpty(jti))
//     {
//       // Явно вказуємо, що саме JTI не знайдено, для кращого дебагінгу
//       throw new SecurityException("Invalid token: JTI claim is missing");
//     }

//     // Перевірка існування refresh токена в базі даних
//     var storedRefreshToken = await _dbContext.RefreshTokens
//         .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

//     if (storedRefreshToken is null)
//     {
//       throw new SecurityException("This refresh token does not exist");
//     }

//     // Перевірка, що термін дії refresh токена ще не закінчився
//     if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
//     {
//       throw new SecurityException("This refresh token has expired");
//     }

//     // Перевірка, що refresh токен не був анульований (відкликаний)
//     if (storedRefreshToken.Invalidated)
//     {
//       throw new SecurityException("This refresh token has been invalidated");
//     }

//     // Переконання, що refresh токен пов'язаний з правильним JWT, порівнюючи ID
//     if (storedRefreshToken.JwtId != jti)
//     {
//       throw new SecurityException("This refresh token does not match this JWT");
//     }

//     // Отримання ID користувача з клеймів
//     var userId = validatedToken.Claims.FirstOrDefault(x => x.Type == "userid")?.Value;
//     if (userId is null)
//     {
//       throw new SecurityException("User ID not found in token");
//     }

//     // Пошук користувача в базі даних
//     var user = await _userManager.FindByIdAsync(userId);
//     if (user is null)
//     {
//       // KeyNotFoundException може бути більш семантично правильним тут
//       throw new KeyNotFoundException($"User with ID '{userId}' not found");
//     }

//     // Створення нової пари токенів (access та refresh)
//     var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, refreshToken);

//     return new RefreshTokenResponse(newToken, newRefreshToken);
//   }

//   private static ClaimsPrincipal? GetPrincipalFromToken(
//       string token,
//       TokenValidationParameters parameters)
//   {
//     var tokenHandler = new JwtSecurityTokenHandler();

//     try
//     {
//       var tokenParameters = parameters.Clone();
//       tokenParameters.ValidateLifetime = false;
//       var principal = tokenHandler.ValidateToken(token, tokenParameters, out var validatedToken);
//       return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
//     }
//     catch
//     {
//       return null;
//     }
//   }

//   private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
//       => validatedToken is JwtSecurityToken jwtSecurityToken
//          && jwtSecurityToken.Header.Alg
//               .Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

//   private async Task<(string token, string refreshToken)> GenerateJwtAndRefreshTokenAsync(
//       User user,
//       string? existingRefreshToken)
//   {
//     var roles = await _userManager.GetRolesAsync(user);
//     var userRole = roles.FirstOrDefault() ?? "user";

//     var role = await _roleManager.FindByNameAsync(userRole);
//     var roleClaims = role is not null ? await _roleManager.GetClaimsAsync(role) : [];

//     var token = GenerateJwtToken(user, _authOptions.Value, userRole, roleClaims);
//     var refreshToken = await GenerateRefreshTokenAsync(token, user, existingRefreshToken);

//     return (token, refreshToken);
//   }

// }