
using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models.Views;

namespace BoardGamesStore.Models.Entities;

public class Product
{
  [Key]
  public int Id { get; set; }

  public required string Name { get; set; }
  public string? Description { get; set; }
  public decimal Price { get; set; }
  public int Stock { get; set; }
  public ICollection<ProductCategory> Categories { get; set; } = [];
  public string? ImageUrl { get; set; }
  public decimal BonusRate { get; set; }
  public decimal MaxBonusPaymentPercent { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public bool IsDeleted { get; set; }

  public ProductRatingSummary? RatingSummary { get; set; }
  public ICollection<Comment>? Comments { get; set; }
  public ICollection<CartItem>? CartItems { get; set; }
  public ICollection<OrderItem>? OrderItems { get; set; }
  public ICollection<SimilarProduct>? SimilarProducts { get; set; }
  public ICollection<SimilarProduct>? RelatedToProducts { get; set; }
  public ICollection<WishlistItem>? WishlistItems { get; set; }
  public ICollection<Evaluation>? Evaluations { get; set; }
  public ICollection<ProductReport>? ProductReports { get; set; }
  public ICollection<ProductImage>? Images { get; set; }
}

