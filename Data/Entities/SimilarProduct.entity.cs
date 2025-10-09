using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class SimilarProduct
{
  [Key]
  public int Id { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public int SimilarProductId { get; set; }
  public required Product SimilarTo { get; set; }
}
