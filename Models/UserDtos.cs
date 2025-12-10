namespace BoardGamesStore.Models;

public class UserDto
{
  public string? Id { get; init; }
  public string? UserName { get; init; }
  public string? Email { get; init; }
  public decimal? BonusBalance { get; init; }
  public string? AvatarUrl { get; init; }
}

public class UserAvatarDto
{
  public string? AvatarUrl { get; init; }
}