using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class Cart
{
  [Key]
  public int Id { get; set; }

  public int UserId { get; set; }
  public required User User { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public ICollection<CartItem>? Items { get; set; }
}

