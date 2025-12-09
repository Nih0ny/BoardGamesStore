
using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models.Entities;

public class ProductReport
{
  [Key]
  public int Id { get; set; }

  public string? UserId { get; set; }
  public User? User { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public string? Reason { get; set; }
  public ReportStatusId StatusId { get; set; }
  public required ReportStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
}