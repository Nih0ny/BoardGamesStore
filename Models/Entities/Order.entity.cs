using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Models.Entities;

public class Order
{
  [Key]
  public int Id { get; private set; }

  public string? UserId { get; set; }
  public User? User { get; set; }

  public OrderStatusId StatusId { get; set; }
  public required OrderStatus Status { get; set; }

  public PaymentStatusId PaymentStatusId { get; set; }
  public required PaymentStatus PaymentStatus { get; set; }

  public DeliveryMethodId DeliveryMethodId { get; set; }
  public required DeliveryMethod DeliveryMethod { get; set; }

  public required Address DeliveryAddress { get; set; }

  public string? TrackingNumber { get; set; }

  [MaxLength(100)]
  public required string RecipientName { get; set; }

  [Phone]
  public required string RecipientPhone { get; set; }

  [EmailAddress]
  public string? RecipientEmail { get; set; }

  public decimal ItemsTotal { get; set; }
  public decimal ShippingCost { get; set; }
  public decimal DiscountAmount { get; set; }

  public decimal Total { get; set; }
  public decimal BonusTotal { get; set; }

  public string? CustomerNote { get; set; }
  public string? AdminNote { get; set; }
  public string? CancellationReason { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime? FinishedAt { get; set; }

  public ICollection<OrderItem>? OrderItems { get; set; }
  public ICollection<PaymentTransaction>? PaymentTransactions { get; set; }
  public ICollection<BonusTransaction>? BonusTransactions { get; set; }
}

[Owned]
public class Address
{
  public string City { get; set; } = string.Empty;
  public string Street { get; set; } = string.Empty;
  public string? Building { get; set; }
  public string? Apartment { get; set; }
  public string? PostalCode { get; set; }
  public string? Region { get; set; }
}
