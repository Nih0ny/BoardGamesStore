using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IOrderService
{
  Task<PagedResult<OrderDto>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);

  Task<OrderDto?> GetByIdAsync(
  int id,
  CancellationToken ct = default);

  Task<PagedResult<OrderDto>> GetByUserIdAsync(
    string userId,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);

  Task<Result<OrderDto>> CreateAsync(
    string userId,
    List<CreateOrderItemDto> items,
    string recipientName,
    string recipientPhone,
    Address deliveryAddress,
    DeliveryMethodId deliveryMethod,
    CancellationToken ct = default);

  Task<Result> ReturnToCartAsync(int orderId, CancellationToken ct = default);

  Task<Result> DeleteAsync(int id, CancellationToken ct = default);

  Task<Result> ChangeStatusAsync(int orderId, OrderStatusId statusId, CancellationToken ct = default);

  Task<Result> ChangePaymentStatusAsync(int orderId, PaymentStatusId statusId, CancellationToken ct = default);
}