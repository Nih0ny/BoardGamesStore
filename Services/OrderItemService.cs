using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ApplicationDbContext _db;

        public OrderItemService(ApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<OrderItem>> GetByOrderAsync(int orderId, CancellationToken ct = default) =>
            await _db.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task<OrderItem> AddOrUpdateAsync(int orderId, int productId, int quantity, decimal price, CancellationToken ct = default)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

            // Load required navs if your model uses 'required'
            var order = await _db.Orders
                .Include(o => o.Status)  // optional
                .Include(o => o.User)    // optional
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
                          ?? throw new KeyNotFoundException($"Product {productId} not found.");

            var existing = await _db.OrderItems.FirstOrDefaultAsync(
                oi => oi.OrderId == orderId && oi.ProductId == productId, ct);

            if (existing is not null)
            {
                existing.Quantity += quantity;
                existing.Price = price; // optional: keep last price or average; adjust as needed
                await _db.SaveChangesAsync(ct);
                // update order total
                await RecalcOrderTotal(orderId, ct);
                return existing;
            }

            var item = new OrderItem
            {
                OrderId = orderId,
                Order = order,           // set required nav if 'required'
                ProductId = productId,
                Product = product,       // set required nav if 'required'
                Quantity = quantity,
                Price = price
            };

            _db.OrderItems.Add(item);
            await _db.SaveChangesAsync(ct);

            await RecalcOrderTotal(orderId, ct);
            return item;
        }

        public async Task<bool> UpdateQuantityAsync(int orderItemId, int quantity, CancellationToken ct = default)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            var item = await _db.OrderItems.FirstOrDefaultAsync(oi => oi.Id == orderItemId, ct);
            if (item is null) return false;

            item.Quantity = quantity;
            await _db.SaveChangesAsync(ct);

            await RecalcOrderTotal(item.OrderId, ct);
            return true;
        }

        public async Task<bool> RemoveAsync(int orderItemId, CancellationToken ct = default)
        {
            var item = await _db.OrderItems.FirstOrDefaultAsync(oi => oi.Id == orderItemId, ct);
            if (item is null) return false;

            var orderId = item.OrderId;
            _db.OrderItems.Remove(item);
            await _db.SaveChangesAsync(ct);

            await RecalcOrderTotal(orderId, ct);
            return true;
        }

        public async Task<bool> ClearAsync(int orderId, CancellationToken ct = default)
        {
            var items = _db.OrderItems.Where(oi => oi.OrderId == orderId);
            if (!await items.AnyAsync(ct)) return true;

            _db.OrderItems.RemoveRange(items);
            await _db.SaveChangesAsync(ct);

            await RecalcOrderTotal(orderId, ct);
            return true;
        }

        private async Task RecalcOrderTotal(int orderId, CancellationToken ct)
        {
            var total = await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => oi.Price * oi.Quantity)
                .SumAsync(ct);

            var order = await _db.Orders.FirstAsync(o => o.Id == orderId, ct);
            order.Total = total;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
