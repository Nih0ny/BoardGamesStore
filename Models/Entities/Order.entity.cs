using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class Order
{
  [Key]
  public int Id { get; set; }

  public required string UserId { get; set; }
  public required User User { get; set; }

  public int StatusId { get; set; }
  public required OrderStatus Status { get; set; }

  public decimal Total { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public ICollection<OrderItem>? OrderItems { get; set; }
  public ICollection<PaymentTransaction>? PaymentTransactions { get; set; }
  public ICollection<BonusTransaction>? BonusTransactions { get; set; }
}
