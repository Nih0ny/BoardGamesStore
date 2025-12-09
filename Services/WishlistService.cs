using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;
namespace BoardGamesStore.Services;

public class WishlistService(ApplicationDbContext context) : IWishlistService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PagedResult<UserWishlistDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
  {
    var query = _context.Users
        .AsNoTracking();

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderBy(u => u.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserWishlistDto
        {
          UserId = u.Id,
          UserEmail = u.Email!,
          Items = u.WishlistItems!.Select(wi => new WishlistItemDto
          {
            Id = wi.Id,
            ProductName = wi.Product.Name,
            ProductId = wi.ProductId,
            Price = wi.Product.Price
          }).ToList()
        })
        .ToListAsync(ct);

    return new PagedResult<UserWishlistDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<UserWishlistDto?> GetByUserIdAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Users.Where(u => u.WishlistItems!.Any())
      .Select(u => new UserWishlistDto
      {
        UserId = u.Id,
        UserEmail = u.Email!,
        Items = u.WishlistItems!.Select(wi => new WishlistItemDto
        {
          Id = wi.Id,
          ProductId = wi.ProductId,
          ProductName = wi.Product.Name,
          Price = wi.Product.Price
        }).ToList(),
      })
      .FirstOrDefaultAsync(u => u.UserId == userId, ct);
  }

  public async Task<Result<AddWishlistItemResponseDto>> AddAsync(string userId, int productId)
  {
    var user = await _context.Users.FindAsync([userId], cancellationToken: default);
    if (user == null) return Result.Fail($"User '{userId}' not found.");

    var product = await _context.Products.FindAsync([productId], cancellationToken: default);
    if (product == null) return Result.Fail($"Product '{productId}' not found.");

    var wishlistItemExists = await _context.WishlistItems
      .AnyAsync(wi => wi.UserId == userId && wi.ProductId == productId);
    if (wishlistItemExists)
      return Result.Fail("Product is already in the wishlist.");

    var wishlistItem = new WishlistItem
    {
      UserId = userId,
      User = user,
      Product = product,
      ProductId = productId,
      AddedAt = DateTime.UtcNow
    };

    _context.WishlistItems.Add(wishlistItem);
    await _context.SaveChangesAsync();

    return Result.Ok(new AddWishlistItemResponseDto
    {
      Id = wishlistItem.Id,
      ProductId = wishlistItem.ProductId,
      ProductName = product.Name,
      Price = product.Price,
      AddedAt = wishlistItem.AddedAt
    });
  }

  public async Task<Result> DeleteAsync(string userId, int productId)
  {
    var wishlistItem = await _context.WishlistItems
      .FirstOrDefaultAsync(wi => wi.UserId == userId && wi.ProductId == productId);

    if (wishlistItem is null)
      return Result.Fail("Product not found in wishlist.");

    _context.WishlistItems.Remove(wishlistItem);
    await _context.SaveChangesAsync();
    return Result.Ok();
  }
}