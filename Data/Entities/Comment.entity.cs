using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class Comment
{
  [Key]
  public int Id { get; set; }

  public int UserId { get; set; }
  public required User User { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public string? Content { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public ICollection<CommentReport>? CommentReports { get; set; }
}
