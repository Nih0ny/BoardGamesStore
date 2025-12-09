using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class Category
{
  [Key]
  public int Id { get; set; }
  public required string Name { get; set; }

  public ICollection<ProductCategory> Products { get; set; } = [];
}