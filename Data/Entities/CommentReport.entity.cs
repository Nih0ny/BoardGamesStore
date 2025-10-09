using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class CommentReport
{
  [Key]
  public int Id { get; set; }

  public int UserId { get; set; }
  public required User User { get; set; }

  public int CommentId { get; set; }
  public required Comment Comment { get; set; }

  public string? Reason { get; set; }
  public required string Status { get; set; }
  public DateTime CreatedAt { get; set; }
}