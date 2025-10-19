using BoardGamesStore.Models;

public class RefreshToken
{
  public string Token { get; set; } = null!;

  public string JwtId { get; set; } = null!;

  public DateTime ExpiryDate { get; set; }

  public bool Invalidated { get; set; }

  public int UserId { get; set; } = 0;

  public User User { get; set; } = null!;
}