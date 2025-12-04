using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;

namespace BoardGamesStore.Services;

public interface ITokenService
{
  Task<string> GenerateJwtTokenAsync(User user);
  Task<string> GenerateRefreshTokenAsync(User user, RefreshToken? oldRefreshToken = null);
  Task<Result<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken);
}