using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models.Entities;

public class DeliveryMethod
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.None)]
  public DeliveryMethodId Id { get; set; }

  [MaxLength(100)]
  public required string Name { get; set; } = string.Empty;

  [MaxLength(500)]
  public string? Description { get; set; } = string.Empty;

  [Column(TypeName = "decimal(18,2)")]
  public decimal BasePrice { get; set; }

  public bool IsActive { get; set; } = true;

  public ICollection<Order>? Orders { get; set; }
}