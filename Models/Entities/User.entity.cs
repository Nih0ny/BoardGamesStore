using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Models.Entities;

public class User : IdentityUser
{
  public decimal Coins { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public bool IsDeleted { get; set; }

  public ICollection<Comment>? Comments { get; set; }
  public ICollection<CartItem>? CartItems { get; set; }
  public ICollection<Order>? Orders { get; set; }
  public ICollection<BonusTransaction>? BonusTransactions { get; set; }
  public ICollection<WishlistItem>? WishlistItems { get; set; }
  public ICollection<Evaluation>? Evaluations { get; set; }
  public ICollection<ProductReport>? ProductReports { get; set; }
  public ICollection<CommentReport>? CommentReports { get; set; }
  public ICollection<RefreshToken>? RefreshTokens { get; set; }
}
