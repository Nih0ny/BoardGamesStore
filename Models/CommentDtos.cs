using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class CommentDto
{
  public int Id { get; init; }
  public string Content { get; init; } = null!;
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
  public string UserId { get; init; } = null!;
  public string UserName { get; init; } = null!;
  public int ProductId { get; init; }
}

public class CreateCommentDto
{
  [Required]
  [StringLength(1000, MinimumLength = 3)]
  public string Content { get; init; } = null!;
}

public class UpdateCommentDto
{
  [Required]
  [StringLength(1000, MinimumLength = 3)]
  public string Content { get; init; } = null!;
}