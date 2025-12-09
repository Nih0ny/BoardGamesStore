using BoardGamesStore.Models.Enums;

namespace BoardGamesStore.Models;

public class OrderDto
{
  public int Id { get; init; }
  public string UserEmail { get; init; } = null!;
  public string? UserId { get; init; }
  public string StatusName { get; init; } = null!;
  public decimal Total { get; init; }
  public decimal BonusTotal { get; init; }
  public DateTime CreatedAt { get; init; }
  public List<OrderItemDto> Items { get; init; } = null!;
}

public class OrderItemDto
{
  public int ProductId { get; init; }
  public string ProductName { get; init; } = null!;
  public int Quantity { get; init; }
  public decimal Price { get; init; }
}

public class CreateOrderDto
{
  public string RecipientName { get; init; } = null!;
  public string RecipientPhone { get; init; } = null!;
  public AdressDto DeliveryAddress { get; init; } = null!;
  public DeliveryMethodId DeliveryMethod { get; init; }
  public List<CreateOrderItemDto> Items { get; init; } = null!;
}

public class CreateOrderItemDto
{
  public int ProductId { get; init; }
  public int Quantity { get; init; }
}

public class AdressDto
{
  public string City { get; init; } = null!;
  public string Street { get; init; } = null!;
  public string? Building { get; init; }
  public string? Apartment { get; init; }
  public string? PostalCode { get; init; }
  public string? Region { get; init; }
}

public class ChangeOrderStatusDto
{
  public OrderStatusId StatusId { get; init; }
}

public class ChangeOrderPaymentStatusDto
{
  public PaymentStatusId StatusId { get; init; }
}

public class AddOrderItemDto
{
  public int ProductId { get; init; }
  public int Quantity { get; init; }
  public decimal Price { get; init; }
}