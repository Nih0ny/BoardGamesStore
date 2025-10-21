using BoardGamesStore.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services
{
    public interface IProductReportService
    {
        Task<List<ProductReport>> GetAllAsync(
            Func<IQueryable<ProductReport>, IIncludableQueryable<ProductReport, object>>? include = null,
            CancellationToken ct = default);

        Task<ProductReport?> GetByIdAsync(
            int id,
            Func<IQueryable<ProductReport>, IIncludableQueryable<ProductReport, object>>? include = null,
            CancellationToken ct = default);

        Task<ProductReport> CreateAsync(string userId, int productId, string reason, string status, DateTime? createdAt = null, CancellationToken ct = default);
        Task<bool> UpdateAsync(ProductReport report, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);

        Task<bool> ChangeStatusAsync(int id, string status, CancellationToken ct = default);
    }
}
