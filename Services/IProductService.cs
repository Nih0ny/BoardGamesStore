using BoardGamesStore.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync(
            Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
            CancellationToken ct = default);

        Task<Product?> GetByIdAsync(
            int id,
            Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
            CancellationToken ct = default);

        Task<Product> CreateAsync(Product product, CancellationToken ct = default);
        Task<bool> UpdateAsync(Product product, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        // Helpers
        Task<bool> AdjustStockAsync(int productId, int delta, CancellationToken ct = default);
        Task<bool> SetImageUrlAsync(int productId, string? imageUrl, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
