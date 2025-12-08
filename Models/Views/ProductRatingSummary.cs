using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BoardGamesStore.Models.Entities;

namespace BoardGamesStore.Models.Views;

public class ProductRatingSummary
{
  [Key]
  public int ProductId { get; set; }
  public double AverageRating { get; set; }
  public int ReviewsCount { get; set; }

  public Product? Product { get; set; }
}