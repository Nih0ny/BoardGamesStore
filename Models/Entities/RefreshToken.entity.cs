using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class RefreshToken
{
  [Key]
  public int Id { get; set; }

  public required string UserId { get; set; }
  public required User User { get; set; }

  public required string Token { get; set; }
  public DateTime Expires { get; set; }
  public bool IsExpired => DateTime.UtcNow >= Expires;
  public DateTime Created { get; set; }
  public DateTime? Revoked { get; set; }
  public bool IsActive => Revoked == null && !IsExpired;
}