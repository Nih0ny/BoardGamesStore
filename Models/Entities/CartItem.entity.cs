using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class CartItem
{
  [Key]
  public int Id { get; set; }

  public int CartId { get; set; }
  public required Cart Cart { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public int Quantity { get; set; }
  public DateTime AddedAt { get; set; }
}
