using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
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
          UserEmail = o.User!.Email!,
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
          UserEmail = o.User!.Email!,
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
          UserEmail = o.User!.Email!,
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

  public async Task<Result<OrderDto>> CreateAsync(
    string userId,
    List<CreateOrderItemDto> items,
    string recipientName,
    string recipientPhone,
    Address deliveryAddress,
    DeliveryMethodId deliveryMethod,
    CancellationToken ct = default)
  {
    if (items == null || items.Count == 0)
      return Result.Fail("Order must contain at least one item.");

    var uniqueItems = items
        .GroupBy(i => i.ProductId)
        .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
        .ToList();

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null) return Result.Fail($"User '{userId}' not found.");

    var status = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == OrderStatusId.New, ct);
    if (status == null) return Result.Fail("OrderStatus 'New' not found.");

    var paymentStatus = await _context.PaymentStatuses.FirstOrDefaultAsync(s => s.Id == PaymentStatusId.Pending, ct);
    if (paymentStatus == null) return Result.Fail("PaymentStatus 'Pending' not found.");

    var deliveryMethodEntity = await _context.DeliveryMethods.FirstOrDefaultAsync(d => d.Id == deliveryMethod, ct);
    if (deliveryMethodEntity == null) return Result.Fail("DeliveryMethod not found.");

    var productIds = uniqueItems.Select(i => i.ProductId).ToList();

    var productsDict = await _context.Products
        .Where(p => productIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id, ct);

    if (productsDict.Count != productIds.Count)
      return Result.Fail("Some products are no longer available.");

    var now = DateTime.UtcNow;

    var order = new Order
    {
      UserId = userId,
      User = user,
      StatusId = OrderStatusId.New,
      Status = status,
      PaymentStatusId = PaymentStatusId.Pending,
      PaymentStatus = paymentStatus,
      DeliveryMethodId = deliveryMethod,
      DeliveryMethod = deliveryMethodEntity,
      CreatedAt = now,
      UpdatedAt = now,
      RecipientName = recipientName,
      RecipientPhone = recipientPhone,
      DeliveryAddress = deliveryAddress,
      OrderItems = []
    };

    decimal itemsTotal = 0;
    decimal calculatedBonusTotal = 0;

    await using var transaction = await _context.Database.BeginTransactionAsync(ct);

    try
    {
      foreach (var item in uniqueItems)
      {
        if (!productsDict.TryGetValue(item.ProductId, out var productEntity))
          return Result.Fail($"Product {item.ProductId} not found.");

        if (productEntity.Stock < item.Quantity)
        {
          return Result.Fail($"Insufficient stock for '{productEntity.Name}'. Available: {productEntity.Stock}");
        }

        productEntity.Stock -= item.Quantity;

        var actualPrice = productEntity.Price;

        var itemBonus = (productEntity.MaxBonusPaymentPercent / 100m) * actualPrice * item.Quantity;
        calculatedBonusTotal += itemBonus;

        var orderItem = new OrderItem
        {
          Product = productEntity,
          Order = order,
          Quantity = item.Quantity,
          Price = actualPrice
        };

        order.OrderItems.Add(orderItem);
        itemsTotal += actualPrice * item.Quantity;
      }

      order.ItemsTotal = itemsTotal;
      order.ShippingCost = deliveryMethodEntity.BasePrice;
      order.Total = itemsTotal + order.ShippingCost - order.DiscountAmount;
      order.BonusTotal = calculatedBonusTotal;

      _context.Orders.Add(order);

      await _context.SaveChangesAsync(ct);
      await transaction.CommitAsync(ct);

    }
    catch (Exception)
    {
      await transaction.RollbackAsync(ct);
      return Result.Fail("Error creating order.");
    }

    return Result.Ok(new OrderDto
    {
      Id = order.Id,
      UserEmail = user.Email!,
      UserId = userId,
      StatusName = status.Name,
      Total = order.Total,
      BonusTotal = order.BonusTotal,
      CreatedAt = order.CreatedAt,
      Items = [.. order.OrderItems.Select(oi => new OrderItemDto
      {
        ProductId = oi.ProductId,
        ProductName = oi.Product.Name,
        Quantity = oi.Quantity,
        Price = oi.Price
      })]
    });
  }

  public async Task<Result> ReturnToCartAsync(int orderId, CancellationToken ct = default)
  {
    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    if (order is null) return Result.Fail("Order not found.");

    var userId = order.UserId;
    if (string.IsNullOrEmpty(userId)) return Result.Fail("User not found.");

    if (order.StatusId > OrderStatusId.New)
      return Result.Fail("Only orders with status 'New' can be returned to cart.");

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

  public async Task<Result> ChangeStatusAsync(int orderId, OrderStatusId statusId, CancellationToken ct = default)
  {
    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
    if (order is null) return Result.Fail("Order not found.");

    var status = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == statusId, ct);
    if (status is null) return Result.Fail("OrderStatus not found.");

    order.StatusId = statusId;
    order.Status = status;
    order.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> ChangePaymentStatusAsync(int orderId, PaymentStatusId statusId, CancellationToken ct = default)
  {
    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
    if (order is null) return Result.Fail("Order not found.");

    var status = await _context.PaymentStatuses.FirstOrDefaultAsync(s => s.Id == statusId, ct);
    if (status is null) return Result.Fail("PaymentStatus not found.");

    order.PaymentStatusId = statusId;
    order.PaymentStatus = status;
    order.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }
}