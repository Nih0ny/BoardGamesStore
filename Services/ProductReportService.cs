using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services
{
    public class ProductReportService : IProductReportService
    {
        private readonly ApplicationDbContext _db;
        public ProductReportService(ApplicationDbContext db) => _db = db;

        public async Task<List<ProductReport>> GetAllAsync(
            Func<IQueryable<ProductReport>, IIncludableQueryable<ProductReport, object>>? include = null,
            CancellationToken ct = default)
        {
            IQueryable<ProductReport> q = _db.ProductReports.AsQueryable();
            if (include is not null) q = include(q);
            return await q.AsNoTracking().ToListAsync(ct);
        }

        public async Task<ProductReport?> GetByIdAsync(
            int id,
            Func<IQueryable<ProductReport>, IIncludableQueryable<ProductReport, object>>? include = null,
            CancellationToken ct = default)
        {
            IQueryable<ProductReport> q = _db.ProductReports.Where(r => r.Id == id);
            if (include is not null) q = include(q);
            return await q.AsNoTracking().FirstOrDefaultAsync(ct);
        }

        public async Task<ProductReport> CreateAsync(string userId, int productId, string reason, string status, DateTime? createdAt = null, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                       ?? throw new KeyNotFoundException($"User '{userId}' not found.");
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
                          ?? throw new KeyNotFoundException($"Product '{productId}' not found.");

            var report = new ProductReport
            {
                UserId = userId,
                User = user,               // set if nav is required
                ProductId = productId,
                Product = product,         // set if nav is required
                Reason = reason,
                Status = status,
                CreatedAt = createdAt ?? DateTime.UtcNow
            };

            _db.ProductReports.Add(report);
            await _db.SaveChangesAsync(ct);
            return report;
        }

        public async Task<bool> UpdateAsync(ProductReport report, CancellationToken ct = default)
        {
            if (!await ExistsAsync(report.Id, ct)) return false;

            // Optional: validate FKs if they can change
            var userExists = await _db.Users.AnyAsync(u => u.Id == report.UserId, ct);
            if (!userExists) throw new KeyNotFoundException($"User '{report.UserId}' not found.");

            var productExists = await _db.Products.AnyAsync(p => p.Id == report.ProductId, ct);
            if (!productExists) throw new KeyNotFoundException($"Product '{report.ProductId}' not found.");

            _db.ProductReports.Update(report);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var r = await _db.ProductReports.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r is null) return false;

            _db.ProductReports.Remove(r);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
            _db.ProductReports.AnyAsync(x => x.Id == id, ct);

        public async Task<bool> ChangeStatusAsync(int id, string status, CancellationToken ct = default)
        {
            var r = await _db.ProductReports.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (r is null) return false;

            r.Status = status;
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
