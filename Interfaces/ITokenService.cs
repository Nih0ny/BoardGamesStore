using BoardGamesStore.Models.Entities;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface ITokenService
{
  Task<string> GenerateJwtTokenAsync(User user);
  Task<string> GenerateRefreshTokenAsync(User user, RefreshToken? oldRefreshToken = null);
  Task<Result<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken);
}