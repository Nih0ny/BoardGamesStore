using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class Comment
{
  [Key]
  public int Id { get; set; }

  public required string UserId { get; set; }
  public User? User { get; set; }

  public int ProductId { get; set; }
  public Product? Product { get; set; }

  public string? Content { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public ICollection<CommentReport>? CommentReports { get; set; }
}
