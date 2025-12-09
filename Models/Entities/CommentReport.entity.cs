using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models.Entities;

public class CommentReport
{
  [Key]
  public int Id { get; set; }

  public string? UserId { get; set; }
  public User? User { get; set; }

  public int CommentId { get; set; }
  public required Comment Comment { get; set; }

  public string? Reason { get; set; }
  public ReportStatusId StatusId { get; set; }
  public required ReportStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
}