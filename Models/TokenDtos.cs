using BoardGamesStore.Models.Entities;

namespace BoardGamesStore.Models;

public class RefreshTokenDto
{
  public string Token { get; init; } = null!;
  public string JwtId { get; init; } = null!;
  public DateTime ExpiryDate { get; init; }
  public bool Invalidated { get; init; }
  public int UserId { get; init; }
  public User User { get; init; } = null!;
}