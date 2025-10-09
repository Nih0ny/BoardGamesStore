
using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class OrderStatus
{
  [Key]
  public int Id { get; set; }
  public required string Status { get; set; }

  public ICollection<Order>? Orders { get; set; }
}