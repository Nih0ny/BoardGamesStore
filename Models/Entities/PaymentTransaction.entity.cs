
using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class PaymentTransaction
{
  [Key]
  public int Id { get; set; }

  public int OrderId { get; set; }
  public required Order Order { get; set; }

  public string? PaymentSystem { get; set; }
  public string? TransactionId { get; set; }
  public decimal Amount { get; set; }
  public string? Status { get; set; }
  public DateTime CreatedAt { get; set; }
}
