using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models;

public class CommentReportDto
{
  public int Id { get; init; }
  public string UserId { get; init; } = null!;
  public int CommentId { get; init; }
  public string? Reason { get; init; }
  public ReportStatusId StatusId { get; init; }
  public DateTime CreatedAt { get; init; }
}

public class UpdateCommentReportDto
{
  public string? UserId { get; init; }
  public int CommentId { get; init; }
  public string Reason { get; init; } = null!;
}

public class CreateCommentReportDto
{
  public string? Reason { get; init; }
}

public class ChangeCommentReportStatusDto
{
  public ReportStatusId Status { get; init; }
}

public class CommentReportStatusChangeDto
{
  public ReportStatusId StatusId { get; init; }
}