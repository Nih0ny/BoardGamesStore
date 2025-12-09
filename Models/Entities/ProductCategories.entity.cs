using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class ProductCategory
{
  [Key]
  public int Id { get; set; }

  public int ProductId { get; set; }
  public Product Product { get; set; } = null!;

  public int CategoryId { get; set; }
  public Category Category { get; set; } = null!;
}