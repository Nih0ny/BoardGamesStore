using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;

namespace BoardGamesStore.Services;

public interface ICartService
{
  Task<PagedResult<UserCartDto>> GetUsersWithCartsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
  Task<UserCartDto?> GetByUserIdAsync(string userId, CancellationToken ct = default);
  Task<Result<CartItem>> AddItemAsync(string userId, int productId, int quantity, CancellationToken ct = default);
  Task<Result> UpdateItemQuantityAsync(string userId, int productId, int quantity, CancellationToken ct = default);
  Task<Result> RemoveItemAsync(string userId, int productId, CancellationToken ct = default);
  Task<Result> ClearAsync(string userId, CancellationToken ct = default);
  Task<int> GetItemsCountAsync(string userId, CancellationToken ct = default);
}
