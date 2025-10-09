using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGamesStore.Models;

public class User
{
  [Key]
  public int Id { get; set; }

  public required string Name { get; set; }
  public required string Email { get; set; }
  public required string Password { get; set; }

  public int RoleId { get; set; }
  public required Role Role { get; set; }

  public decimal Coins { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public ICollection<Comment>? Comments { get; set; }
  public ICollection<Cart>? Carts { get; set; }
  public ICollection<Order>? Orders { get; set; }
  public ICollection<BonusTransaction>? BonusTransactions { get; set; }
  public ICollection<Wishlist>? Wishlists { get; set; }
  public ICollection<Evaluation>? Evaluations { get; set; }
  public ICollection<ProductReport>? ProductReports { get; set; }
  public ICollection<CommentReport>? CommentReports { get; set; }
}
