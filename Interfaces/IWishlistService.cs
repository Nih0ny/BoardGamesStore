using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IWishlistService
{
  Task<UserWishlistDto?> GetByUserIdAsync(string userId, CancellationToken ct = default);
  Task<PagedResult<UserWishlistDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
  Task<Result<AddWishlistItemResponseDto>> AddAsync(string userId, int productId);
  Task<Result> DeleteAsync(string userId, int productId);
}