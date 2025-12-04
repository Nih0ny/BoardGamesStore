using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services;

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

  Task<Result<Order>> CreateAsync(string userId, int statusId, CancellationToken ct = default);

  Task<Result> ReturnToCartAsync(int orderId, CancellationToken ct = default);

  Task<Result> DeleteAsync(int id, CancellationToken ct = default);

  Task<Result> ChangeStatusAsync(int orderId, int statusId, CancellationToken ct = default);
}