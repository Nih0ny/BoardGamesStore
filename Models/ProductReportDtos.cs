namespace BoardGamesStore.Models;

public class ProductReportDto
{
  public int ProductId { get; set; }
  public string ProductName { get; set; } = null!;
  public string? Reason { get; set; }
  public UserDto? ReportedBy { get; set; }
}