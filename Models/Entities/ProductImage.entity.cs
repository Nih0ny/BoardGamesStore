using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models.Entities;

public class ProductImage
{
  [Key]
  public int Id { get; set; }

  public int ProductId { get; set; }
  public required Product Product { get; set; }

  public required string FileName { get; set; }
  public required string RelativePath { get; set; }
  public bool IsMainImage { get; set; }
  public int DisplayOrder { get; set; }
  public DateTime CreatedAt { get; set; }
}
