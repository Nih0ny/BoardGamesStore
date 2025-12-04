namespace BoardGamesStore.Models;

public class UserCartDto
{
  public required string UserId { get; set; }
  public required string UserEmail { get; set; }
  public required List<CartItemDto> Items { get; set; }
  public decimal TotalSum { get; set; }
}

public class CartItemDto
{
  public int ProductId { get; set; }
  public required string ProductName { get; set; }
  public int Quantity { get; set; }
  public decimal Price { get; set; }
}