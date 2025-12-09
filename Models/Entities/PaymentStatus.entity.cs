using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models.Entities;

public class PaymentStatus
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
  public PaymentStatusId Id { get; set; }
  [Required]
  public required string Name { get; set; } = string.Empty;

  public ICollection<Order>? Orders { get; set; }
}