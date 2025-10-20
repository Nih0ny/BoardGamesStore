using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class RefreshToken
{
  [Key]
  public int Id { get; set; }

  public required string UserId { get; set; } // ID користувача з Identity
  public required User User { get; set; } // Навігаційна властивість до користувача

  public required string Token { get; set; }
  public DateTime Expires { get; set; }
  public bool IsExpired => DateTime.UtcNow >= Expires;
  public DateTime Created { get; set; }
  public DateTime? Revoked { get; set; } // Час відкликання
  public bool IsActive => Revoked == null && !IsExpired;
}