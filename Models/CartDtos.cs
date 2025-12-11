namespace BoardGamesStore.Models;

public class UserCartDto
{
  public string UserId { get; init; } = null!;
  public string UserEmail { get; init; } = null!;
  public List<CartItemDto> Items { get; init; } = null!;
  public decimal TotalSum { get; init; }
}

public class CartItemDto
{
  public int ProductId { get; init; }
  public string ProductName { get; init; } = null!;
  public int Quantity { get; init; }
  public decimal Price { get; init; }
  public string ImageUrl { get; init; } = null!;
}

public class AddCartItemDto
{
  public int ProductId { get; init; }
  public int Quantity { get; init; }
}

public class CartItemQuantityUpdateDto
{
  public int Quantity { get; init; }
}