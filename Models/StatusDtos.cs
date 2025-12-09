namespace BoardGamesStore.Models;

public class OrderStatusDto
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
}

public class PaymentStatusDto
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
}

public class ReportStatusDto
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
}

public class DeliveryMethodDto
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
  public string? Description { get; init; }
  public decimal BasePrice { get; init; }
  public bool IsActive { get; init; }
}
