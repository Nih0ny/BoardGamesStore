namespace BoardGamesStore.Models;

public class OrderDto
{
  public int Id { get; set; }
  public required string UserEmail { get; set; }
  public required string StatusName { get; set; }
  public decimal Total { get; set; }
  public decimal BonusTotal { get; set; }
  public DateTime CreatedAt { get; set; }
  public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
  public int ProductId { get; set; }
  public required string ProductName { get; set; }
  public int Quantity { get; set; }
  public decimal Price { get; set; }
}