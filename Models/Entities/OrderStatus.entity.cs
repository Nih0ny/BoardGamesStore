
using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class OrderStatus
{
  [Key]
  public int Id { get; set; }
  public required string Name { get; set; }

  public ICollection<Order>? Orders { get; set; }
}