using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;

        public OrderService(ApplicationDbContext db) => _db = db;

        public async Task<List<Order>> GetAllAsync(
            Func<IQueryable<Order>, IIncludableQueryable<Order, object>>? include = null,
            CancellationToken ct = default)
        {
            IQueryable<Order> q = _db.Orders.AsQueryable();
            if (include is not null) q = include(q);
            return await q.AsNoTracking().ToListAsync(ct);
        }

        public async Task<Order?> GetByIdAsync(
            int id,
            Func<IQueryable<Order>, IIncludableQueryable<Order, object>>? include = null,
            CancellationToken ct = default)
        {
            IQueryable<Order> q = _db.Orders.Where(o => o.Id == id);
            if (include is not null) q = include(q);
            return await q.AsNoTracking().FirstOrDefaultAsync(ct);
        }

        public async Task<Order> CreateAsync(string userId, int statusId, decimal total, CancellationToken ct = default)
        {
            // Load required navs if your model uses 'required' on them
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                       ?? throw new KeyNotFoundException($"User '{userId}' not found.");

            var status = await _db.OrderStatuses.FirstOrDefaultAsync(s => s.Id == statusId, ct)
                         ?? throw new KeyNotFoundException($"OrderStatus '{statusId}' not found.");

            var now = DateTime.UtcNow;

            var order = new Order
            {
                UserId = userId,
                User = user,            // set required nav if 'required User User'
                StatusId = statusId,
                Status = status,        // set required nav if 'required OrderStatus Status'
                Total = total,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);
            return order;
        }

        public async Task<bool> UpdateAsync(Order order, CancellationToken ct = default)
        {
            // Optionally validate related keys exist (UserId string!)
            var userExists = await _db.Users.AnyAsync(u => u.Id == order.UserId, ct);
            if (!userExists) throw new KeyNotFoundException($"User '{order.UserId}' not found.");

            var statusExists = await _db.OrderStatuses.AnyAsync(s => s.Id == order.StatusId, ct);
            if (!statusExists) throw new KeyNotFoundException($"OrderStatus '{order.StatusId}' not found.");

            order.UpdatedAt = DateTime.UtcNow;
            _db.Orders.Update(order);

            try
            {
                await _db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ExistsAsync(order.Id, ct);
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (order is null) return false;

            // If cascade delete isn't configured, remove items first
            var items = _db.OrderItems.Where(oi => oi.OrderId == id);
            _db.OrderItems.RemoveRange(items);

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
            _db.Orders.AnyAsync(o => o.Id == id, ct);

        public async Task<decimal> RecalculateTotalAsync(int orderId, bool save = true, CancellationToken ct = default)
        {
            var items = await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new { oi.Price, oi.Quantity })
                .ToListAsync(ct);

            var total = items.Sum(i => i.Price * i.Quantity);

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct)
                        ?? throw new KeyNotFoundException($"Order {orderId} not found.");
            order.Total = total;
            order.UpdatedAt = DateTime.UtcNow;

            if (save)
                await _db.SaveChangesAsync(ct);

            return total;
        }

        public async Task<bool> ChangeStatusAsync(int orderId, int statusId, CancellationToken ct = default)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
            if (order is null) return false;

            var status = await _db.OrderStatuses.FirstOrDefaultAsync(s => s.Id == statusId, ct)
                         ?? throw new KeyNotFoundException($"OrderStatus '{statusId}' not found.");

            order.StatusId = statusId;
            order.Status = status; // set nav if 'required'
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
