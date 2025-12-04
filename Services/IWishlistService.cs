using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;

namespace BoardGamesStore.Services;

public interface IWishlistService
{
  Task<UserWishlistDto?> GetByUserIdAsync(string userId, CancellationToken ct = default);
  Task<PagedResult<UserWishlistDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
  Task<Result<WishlistItem>> AddAsync(string userId, int productId);
  Task<Result> DeleteAsync(int wishlistItemId);
}