using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Services.Settings;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BoardGamesStore.Services;

public class TokenService(
    UserManager<User> userManager,
    IOptions<JwtSettings> jwtSettings,
    ApplicationDbContext context) : ITokenService
{
  private readonly UserManager<User> _userManager = userManager;
  private readonly JwtSettings _jwtSettings = jwtSettings.Value;
  private readonly ApplicationDbContext _context = context;

  public async Task<string> GenerateJwtTokenAsync(User user)
  {
    var userRoles = await _userManager.GetRolesAsync(user);

    Console.WriteLine("JWT Settings: ", _jwtSettings);
    var authClaims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email!),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
        new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer)
    };
    authClaims.AddRange(userRoles.Select(role => new Claim("role", role)));

    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

    var token = new SecurityTokenDescriptor
    {
      Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings.TokenExpirationMinutes)),
      Subject = new ClaimsIdentity(authClaims),
      SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var securityToken = tokenHandler.CreateToken(token);
    return tokenHandler.WriteToken(securityToken);
  }

  public async Task<string> GenerateRefreshTokenAsync(User user, RefreshToken? oldRefreshToken = null)
  {
    if (oldRefreshToken != null)
    {
      oldRefreshToken.Revoked = DateTime.UtcNow;
    }
    var newRefreshTokenEntity = new RefreshToken
    {
      UserId = user.Id,
      User = user,
      Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
      Expires = DateTime.UtcNow.AddDays(90),
      Created = DateTime.UtcNow
    };
    _context.RefreshTokens.Add(newRefreshTokenEntity);
    await _context.SaveChangesAsync();
    return newRefreshTokenEntity.Token;
  }

  public async Task<Result<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken)
  {
    var refreshTokenEntity = await _context.RefreshTokens.Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.Revoked.HasValue);

    if (refreshTokenEntity == null || refreshTokenEntity.IsExpired || refreshTokenEntity.User == null)
    {
      return Result.Fail<(string AccessToken, string RefreshToken)>("Invalid refresh token");
    }

    var newAccessToken = await GenerateJwtTokenAsync(refreshTokenEntity.User);
    var newRefreshToken = await GenerateRefreshTokenAsync(refreshTokenEntity.User, refreshTokenEntity);

    refreshTokenEntity.Revoked = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    return Result.Ok((newAccessToken, newRefreshToken));
  }
}