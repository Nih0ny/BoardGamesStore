namespace BoardGamesStore.Models;

public class UserWishlistDto
{
  public string UserId { get; init; } = null!;
  public string UserEmail { get; init; } = null!;
  public List<WishlistItemDto> Items { get; init; } = null!;
}

public class WishlistItemDto
{
  public int Id { get; init; }
  public int ProductId { get; init; }
  public string ProductName { get; init; } = null!;
  public decimal Price { get; init; }
}

public class CreateWishlistItemDto
{
  public int ProductId { get; init; }
}

public class AddWishlistItemResponseDto
{
  public int Id { get; init; }
  public int ProductId { get; init; }
  public string ProductName { get; init; } = null!;
  public decimal Price { get; init; }
  public DateTime AddedAt { get; init; }
}