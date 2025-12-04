using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Services;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services;

public class OrderService(ApplicationDbContext context, ICartService cartService) : IOrderService
{
  private readonly ApplicationDbContext _context = context;
  private readonly ICartService _cartService = cartService;

  public async Task<PagedResult<OrderDto>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
  {
    var query = _context.Orders.AsNoTracking();

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(o => new OrderDto
        {
          Id = o.Id,
          UserEmail = o.User.Email!,
          StatusName = o.Status.Name,
          Total = o.Total,
          BonusTotal = o.BonusTotal,
          CreatedAt = o.CreatedAt,
          Items = o.OrderItems!.Select(oi => new OrderItemDto
          {
            ProductName = oi.Product.Name,
            Quantity = oi.Quantity,
            Price = oi.Price
          }).ToList()
        })
        .ToListAsync(ct);

    return new PagedResult<OrderDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<OrderDto?> GetByIdAsync(
    int id,
    CancellationToken ct = default)
  {
    return await _context.Orders
        .AsNoTracking()
        .Where(o => o.Id == id)
        .Select(o => new OrderDto
        {
          Id = o.Id,
          UserEmail = o.User.Email!,
          StatusName = o.Status.Name,
          Total = o.Total,
          BonusTotal = o.BonusTotal,
          CreatedAt = o.CreatedAt,
          Items = o.OrderItems!.Select(oi => new OrderItemDto
          {
            ProductId = oi.ProductId,
            ProductName = oi.Product.Name,
            Quantity = oi.Quantity,
            Price = oi.Price
          }).ToList()
        })
        .FirstOrDefaultAsync(ct);
  }

  public async Task<PagedResult<OrderDto>> GetByUserIdAsync(
    string userId,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
  {
    var query = _context.Orders
        .AsNoTracking()
        .Where(o => o.UserId == userId);

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(o => new OrderDto
        {
          Id = o.Id,
          UserEmail = o.User.Email!,
          StatusName = o.Status.Name,
          Total = o.Total,
          BonusTotal = o.BonusTotal,
          CreatedAt = o.CreatedAt,
          Items = o.OrderItems!.Select(oi => new OrderItemDto
          {
            ProductId = oi.ProductId,
            ProductName = oi.Product.Name,
            Quantity = oi.Quantity,
            Price = oi.Price
          }).ToList()
        })
        .ToListAsync(ct);

    return new PagedResult<OrderDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<Result<Order>> CreateAsync(string userId, int statusId, CancellationToken ct = default)
  {
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null) return Result.Fail($"User '{userId}' not found.");

    var status = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == statusId, ct);
    if (status == null) return Result.Fail($"OrderStatus '{statusId}' not found.");

    var cart = await _cartService.GetByUserIdAsync(user.Id, ct);
    if (cart == null || cart.Items.Count == 0) return Result.Fail("Cart is empty. Cannot create order.");

    var productIds = cart.Items.Select(ci => ci.ProductId).Distinct().ToList();

    var productsDict = await _context.Products
        .Where(p => productIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id, ct);

    if (productsDict.Count != productIds.Count) return Result.Fail("Some products in the cart no longer exist.");


    var now = DateTime.UtcNow;
    var bonusTotal = cart.Items.Sum(item =>
    {
      if (productsDict.TryGetValue(item.ProductId, out var product)) return product.MaxBonusPaymentPercent / 100m * item.Price * item.Quantity;
      return 0m;
    });

    var order = new Order
    {
      UserId = userId,
      User = user,
      StatusId = statusId,
      Status = status,
      BonusTotal = bonusTotal,
      CreatedAt = now,
      UpdatedAt = now
    };

    var orderItems = new List<OrderItem>();
    decimal calculatedTotal = 0;

    foreach (var item in cart.Items)
    {
      if (!productsDict.TryGetValue(item.ProductId, out var productEntity))
      {
        continue;
      }

      var orderItem = new OrderItem
      {
        ProductId = item.ProductId,
        Product = productEntity,
        Order = order,
        Quantity = item.Quantity,
        Price = item.Price
      };

      // FIXME: Check stock and reduce it before addin in order
      orderItems.Add(orderItem);
      calculatedTotal += orderItem.Price * orderItem.Quantity;
    }

    order.OrderItems = orderItems;
    order.Total = calculatedTotal;

    using var transaction = await _context.Database.BeginTransactionAsync(ct);
    try
    {
      _context.Orders.Add(order);
      await _context.SaveChangesAsync(ct);

      await _cartService.ClearAsync(userId, ct);

      await transaction.CommitAsync(ct);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(ct);
      return Result.Fail("Error creating order");
    }

    return order;
  }

  public async Task<Result> ReturnToCartAsync(int orderId, CancellationToken ct = default)
  {
    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    if (order is null) return Result.Fail("Order not found.");

    var userId = order.UserId;

    using var transaction = await _context.Database.BeginTransactionAsync(ct);
    try
    {
      foreach (var item in order.OrderItems!)
      {
        var addResult = await _cartService.AddItemAsync(userId, item.ProductId, item.Quantity, ct);
        if (addResult.IsFailed)
        {
          await transaction.RollbackAsync(ct);
          return Result.Fail($"Failed to add product {item.ProductId} to cart: {string.Join(", ", addResult.Errors.Select(e => e.Message))}");
        }
      }

      await transaction.CommitAsync(ct);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(ct);
      return Result.Fail("Error returning items to cart");
    }

    return Result.Ok();
  }

  public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
  {
    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
    if (order is null) return Result.Fail("Order not found.");

    _context.Orders.Remove(order);
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> ChangeStatusAsync(int orderId, int statusId, CancellationToken ct = default)
  {
    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
    if (order is null) return Result.Fail("Order not found.");

    var status = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == statusId, ct);
    if (status is null) return Result.Fail("OrderStatus not found.");

    order.StatusId = statusId;
    order.Status = status; // set nav if 'required'
    order.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }
}