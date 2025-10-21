using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;
        public CartService(ApplicationDbContext db) => _db = db;

        public async Task<List<Cart>> GetAllAsync(CancellationToken ct = default) =>
            await _db.Carts.Include(c => c.User).AsNoTracking().ToListAsync(ct);

        public async Task<Cart?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.Carts.Include(c => c.User).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

        // string userId!
        public async Task<Cart> CreateAsync(string userId, DateTime? createdAt = null, CancellationToken ct = default)
        {
            // load the user so we can set the required navigation property
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null) throw new KeyNotFoundException($"User '{userId}' not found.");

            var now = DateTime.UtcNow;
            var cart = new Cart
            {
                UserId = userId,   // string
                User = user,       // set required nav
                CreatedAt = createdAt ?? now,
                UpdatedAt = now
            };

            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);
            return cart;
        }

        public async Task<bool> UpdateAsync(Cart cart, CancellationToken ct = default)
        {
            // ensure user still exists (string id)
            var userExists = await _db.Users.AnyAsync(u => u.Id == cart.UserId, ct);
            if (!userExists) throw new KeyNotFoundException($"User '{cart.UserId}' not found.");

            cart.UpdatedAt = DateTime.UtcNow;
            _db.Carts.Update(cart);

            try
            {
                await _db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ExistsAsync(cart.Id, ct);
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cart is null) return false;

            var items = _db.CartItems.Where(ci => ci.CartId == id);
            _db.CartItems.RemoveRange(items);

            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
            _db.Carts.AnyAsync(c => c.Id == id, ct);
    }
}
