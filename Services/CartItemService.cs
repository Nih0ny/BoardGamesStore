using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly ApplicationDbContext _db;

        public CartItemService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CartItem>> GetItemsAsync(int cartId, CancellationToken ct = default)
        {
            return await _db.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartId)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<CartItem> AddItemAsync(int cartId, int productId, int quantity, CancellationToken ct = default)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            var cart = await _db.Carts
                .Include(c => c.User) // optional
                .FirstOrDefaultAsync(c => c.Id == cartId, ct);
            if (cart is null) throw new KeyNotFoundException($"Cart {cartId} not found.");

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
            if (product is null) throw new KeyNotFoundException($"Product {productId} not found.");

            // upsert path
            var existing = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId, ct);

            if (existing is not null)
            {
                existing.Quantity += quantity;
                existing.AddedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
                return existing;
            }

            var item = new CartItem
            {
                CartId = cartId,
                Cart = cart,             // <-- required nav set
                ProductId = productId,
                Product = product,       // <-- required nav set
                Quantity = quantity,
                AddedAt = DateTime.UtcNow
            };

            _db.CartItems.Add(item);
            await _db.SaveChangesAsync(ct);
            return item;
        }


        public async Task<bool> UpdateQuantityAsync(int cartItemId, int quantity, CancellationToken ct = default)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId, ct);
            if (item == null) return false;

            // Optional stock check:
            // var productStock = await _db.Products.Where(p => p.Id == item.ProductId).Select(p => p.Stock).FirstAsync(ct);
            // if (quantity > productStock) throw new InvalidOperationException("Insufficient stock.");

            item.Quantity = quantity;
            item.AddedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RemoveItemAsync(int cartItemId, CancellationToken ct = default)
        {
            var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId, ct);
            if (item == null) return false;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ClearAsync(int cartId, CancellationToken ct = default)
        {
            var items = _db.CartItems.Where(ci => ci.CartId == cartId);
            if (!await items.AnyAsync(ct)) return true;

            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetItemCountAsync(int cartId, CancellationToken ct = default)
        {
            return await _db.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => (int?)ci.Quantity, ct) ?? 0;
        }
    }
}
