using BoardGamesStore.Models;

namespace BoardGamesStore.Services;

public interface ITokenService
{
  Task<string> GenerateJwtTokenAsync(User user);
  Task<string> GenerateRefreshTokenAsync(User user, RefreshToken? oldRefreshToken = null);
}