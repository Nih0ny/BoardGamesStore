using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class CommentDto
{
  public int Id { get; set; }
  public string Content { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string UserId { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public int ProductId { get; set; }
}

public class CreateCommentDto
{
  [Required]
  [StringLength(1000, MinimumLength = 3)]
  public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
  [Required]
  [StringLength(1000, MinimumLength = 3)]
  public string Content { get; set; } = string.Empty;
}