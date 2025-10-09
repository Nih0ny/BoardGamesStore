using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class OrderItem
{
  [Key]
  public int Id { get; set; }

  public int OrderId { get; set; }
  public required Order Order { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public int Quantity { get; set; }
  public decimal Price { get; set; }
}
