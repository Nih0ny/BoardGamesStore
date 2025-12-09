namespace BoardGamesStore.Models;

public class ProductReportDto
{
  public int Id { get; init; }
  public int ProductId { get; init; }
  public string ProductName { get; init; } = null!;
  public string? Reason { get; init; }
  public string Status { get; init; } = null!;
  public UserDto? ReportedBy { get; init; }
}

public class UpdateProductReportDto
{
  public string? Reason { get; init; }
}

public class ChangeProductReportStatusDto
{
  public string Status { get; init; } = null!;
}

public class CreateProductReportDto
{
  public int ProductId { get; init; }
  public string? Reason { get; init; }
}