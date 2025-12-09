using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models.Entities;

public class ReportStatus
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
  public ReportStatusId Id { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty;
  public ICollection<ProductReport>? ProductReports { get; set; }
  public ICollection<CommentReport>? CommentReports { get; set; }
}
