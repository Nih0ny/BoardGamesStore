using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Services;
using FluentResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class CartService(ApplicationDbContext context) : ICartService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PagedResult<UserCartDto>> GetUsersWithCartsAsync(int pageNumber, int pageSize, CancellationToken ct = default)
  {
    var query = _context.Users
        .AsNoTracking()
        .Where(u => u.CartItems!.Any());

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderBy(u => u.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserCartDto
        {
          UserId = u.Id,
          UserEmail = u.Email!,
          Items = u.CartItems!.Select(ci => new CartItemDto
          {
            ProductName = ci.Product.Name,
            ProductId = ci.ProductId,
            Quantity = ci.Quantity,
            Price = ci.Product.Price
          }).ToList(),
          TotalSum = u.CartItems!.Sum(ci => ci.Product.Price * ci.Quantity)
        })
        .AsSplitQuery()
        .ToListAsync(ct);

    return new PagedResult<UserCartDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<UserCartDto?> GetByUserIdAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Users
      .Where(u => u.CartItems!.Any())
      .Select(u => new UserCartDto
      {
        UserId = u.Id,
        UserEmail = u.Email!,
        Items = u.CartItems!.Select(ci => new CartItemDto
        {
          ProductId = ci.ProductId,
          ProductName = ci.Product.Name,
          Quantity = ci.Quantity,
          Price = ci.Product.Price
        }).ToList(),
        TotalSum = u.CartItems!.Sum(ci => ci.Product.Price * ci.Quantity)
      })
      .FirstOrDefaultAsync(u => u.UserId == userId, ct);
  }

  // FIXME: Deprecated
  // public async Task<Cart?> CreateAsync(string email, CancellationToken ct = default)
  // {
  //     // load the user so we can set the required navigation property
  //     var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
  //     if (user is null) return null;

  //     var cart = new Cart
  //     {
  //         UserId = user.Id,
  //         User = user,
  //         CreatedAt = DateTime.UtcNow,
  //         UpdatedAt = DateTime.UtcNow
  //     };

  //     _context.Carts.Add(cart);
  //     await _context.SaveChangesAsync(ct);
  //     return cart;
  // }

  // public async Task<bool> UpdateAsync(Cart cart, CancellationToken ct = default)
  // {
  //     // ensure user still exists (string id)
  //     var userExists = await _context.Users.AnyAsync(u => u.Id == cart.UserId, ct);
  //     if (!userExists) throw new KeyNotFoundException($"User '{cart.UserId}' not found.");

  //     cart.UpdatedAt = DateTime.UtcNow;
  //     _context.Carts.Update(cart);

  //     try
  //     {
  //         await _context.SaveChangesAsync(ct);
  //         return true;
  //     }
  //     catch (DbUpdateConcurrencyException)
  //     {
  //         return await ExistsAsync(cart.Id, ct);
  //     }
  // }

  // public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
  // {
  //     var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id, ct);
  //     if (cart is null) return false;

  //     var items = _context.CartItems.Where(ci => ci.CartId == id);
  //     _context.CartItems.RemoveRange(items);

  //     _context.Carts.Remove(cart);
  //     await _context.SaveChangesAsync(ct);
  //     return true;
  // }

  // public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
  //     _context.Carts.AnyAsync(c => c.Id == id, ct);

  public async Task<Result<CartItem>> AddItemAsync(string userId, int productId, int quantity, CancellationToken ct = default)
  {
    if (quantity < 1) return Result.Fail<CartItem>("Quantity must be at least 1.");

    if (await _context.CartItems.AnyAsync(ci => ci.UserId == userId && ci.ProductId == productId, ct))
      return Result.Fail<CartItem>("Cart item already exists. Use update instead.");

    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product is null) return Result.Fail<CartItem>("Product not found.");

    var item = new CartItem
    {
      UserId = userId,
      User = null!,             // <-- required nav set
      ProductId = productId,
      Product = product,       // <-- required nav set
      Quantity = quantity,
      AddedAt = DateTime.UtcNow
    };

    _context.CartItems.Add(item);
    await _context.SaveChangesAsync(ct);
    return Result.Ok(item);
  }

  public async Task<Result> UpdateItemQuantityAsync(string userId, int productId, int quantity, CancellationToken ct = default)
  {
    if (quantity < 1) return Result.Fail("Quantity must be at least 1.");

    var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, ct);
    if (item is null) return Result.Fail("Cart item not found.");

    item.Quantity = quantity;
    item.AddedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> RemoveItemAsync(string userId, int productId, CancellationToken ct = default)
  {
    var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, ct);
    if (item is null) return Result.Fail("Cart item not found.");

    _context.CartItems.Remove(item);
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> ClearAsync(string userId, CancellationToken ct = default)
  {
    var items = _context.CartItems.Where(ci => ci.UserId == userId);
    if (!await items.AnyAsync(ct)) return Result.Ok();

    _context.CartItems.RemoveRange(items);
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<int> GetItemsCountAsync(string userId, CancellationToken ct = default)
  {
    return await _context.CartItems
        .Where(ci => ci.UserId == userId)
        .SumAsync(ci => (int?)ci.Quantity, ct) ?? 0;
  }
}
