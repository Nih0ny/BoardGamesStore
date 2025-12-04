using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services;

public record ProductQuery(
    string? Text,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool InStockOnly,
    string? Sort,         // "price_asc","price_desc","newest","name","bonus_desc"
    int Skip,
    int Take
);

public record CategoryCount(string Category, int Count);

public interface IProductService
{
    Task<PagedResult<Product>> GetAllAsync(
        int pageNumber,
        int pageSize,
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

    // New query-style endpoints
    Task<PagedResult<Product>> SearchAsync(ProductQuery query, CancellationToken ct = default);
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<List<Product>> GetSimilarAsync(int productId, int limit, CancellationToken ct = default);
    Task<List<Product>> GetNewestAsync(int limit, string? category = null, CancellationToken ct = default);
    Task<List<CategoryCount>> GetCategoriesWithCountsAsync(CancellationToken ct = default);
    Task<(decimal min, decimal max, int count)> GetPriceStatsAsync(string? category = null, CancellationToken ct = default);
    Task<bool> SetDiscountAsync(int productId, decimal discountPercentage, CancellationToken ct = default);
}

