
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models.Entities;

public class OrderStatus
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
  public OrderStatusId Id { get; set; }
  [Required]
  public required string Name { get; set; } = string.Empty;

  public ICollection<Order>? Orders { get; set; }
}
