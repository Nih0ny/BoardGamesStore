
using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class ProductReport
{
  [Key]
  public int Id { get; set; }

  public required string UserId { get; set; }
  public required User User { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public string? Reason { get; set; }
  public required string Status { get; set; }
  public DateTime CreatedAt { get; set; }
}