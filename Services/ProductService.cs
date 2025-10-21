using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;
        public ProductService(ApplicationDbContext db) => _db = db;

        public async Task<List<Product>> GetAllAsync(
            Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
            CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products.AsQueryable();
            if (include is not null) q = include(q);
            return await q.AsNoTracking().ToListAsync(ct);
        }

        public async Task<Product?> GetByIdAsync(
            int id,
            Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
            CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products.Where(p => p.Id == id);
            if (include is not null) q = include(q);
            return await q.AsNoTracking().FirstOrDefaultAsync(ct);
        }

        public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            product.CreatedAt = now;
            product.UpdatedAt = now;

            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);
            return product;
        }

        public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
        {
            if (!await ExistsAsync(product.Id, ct)) return false;

            product.UpdatedAt = DateTime.UtcNow;
            _db.Products.Update(product);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return false;

            _db.Products.Remove(p);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
            _db.Products.AnyAsync(p => p.Id == id, ct);

        public async Task<bool> AdjustStockAsync(int productId, int delta, CancellationToken ct = default)
        {
            var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId, ct);
            if (p is null) return false;

            checked { p.Stock += delta; } // throws on overflow
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> SetImageUrlAsync(int productId, string? imageUrl, CancellationToken ct = default)
        {
            var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId, ct);
            if (p is null) return false;

            p.ImageUrl = imageUrl;
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
