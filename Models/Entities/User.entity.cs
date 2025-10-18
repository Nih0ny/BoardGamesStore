using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Models;

public class User : IdentityUser<int>
{

  public required string Name { get; set; }
  public required string Password { get; set; }

  public ICollection<UserRole>? UserRoles { get; set; }

  public decimal Coins { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public ICollection<Comment>? Comments { get; set; }
  public Cart? Cart { get; set; }
  public ICollection<Order>? Orders { get; set; }
  public ICollection<BonusTransaction>? BonusTransactions { get; set; }
  public ICollection<Wishlist>? Wishlists { get; set; }
  public ICollection<Evaluation>? Evaluations { get; set; }
  public ICollection<ProductReport>? ProductReports { get; set; }
  public ICollection<CommentReport>? CommentReports { get; set; }
}
