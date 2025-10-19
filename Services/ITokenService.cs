namespace BoardGamesStore.Services;

public interface ITokenService
{
  string GenerateAccessToken(string userId, string userEmail);
  Task<string> GenerateRefreshTokenAsyn();
}