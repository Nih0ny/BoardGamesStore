using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class Evaluation
{
  [Key]
  public int Id { get; set; }

  public required string UserId { get; set; }
  public User? User { get; set; }

  public int ProductId { get; set; }
  public Product? Product { get; set; }

  public int Rating { get; set; }
  public DateTime CreatedAt { get; set; }
}
