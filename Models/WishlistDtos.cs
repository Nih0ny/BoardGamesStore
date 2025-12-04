namespace BoardGamesStore.Models;

public class UserWishlistDto
{
  public required string UserId { get; set; }
  public required string UserEmail { get; set; }
  public required List<WishlistItemDto> Items { get; set; }
}

public class WishlistItemDto
{
  public int ProductId { get; set; }
  public required string ProductName { get; set; }
  public decimal Price { get; set; }
}