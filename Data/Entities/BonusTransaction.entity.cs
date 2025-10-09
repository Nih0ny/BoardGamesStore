using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class BonusTransaction
{
  [Key]
  public int Id { get; set; }

  public int UserId { get; set; }
  public required User User { get; set; }

  public int? OrderId { get; set; }
  public required Order Order { get; set; }

  public decimal Amount { get; set; }
  public required string Type { get; set; } // Accrual / Usage
  public DateTime CreatedAt { get; set; }
}
