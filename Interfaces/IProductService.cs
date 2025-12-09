using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductSearchQuery query, CancellationToken ct = default);

    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<List<CategoryCount>> GetCategoryStatsAsync(CancellationToken ct = default);
    Task<(decimal min, decimal max)> GetPriceRangeAsync(string? category = null, CancellationToken ct = default);

    Task<List<ProductDto>> GetSimilarAsync(int productId, int limit = 5, CancellationToken ct = default);

    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default);

    Task<Result> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<Result> AdjustStockAsync(int productId, int quantityChange, CancellationToken ct = default);

    Task<Result> SetDiscountAsync(int productId, SetProductDiscountDto dto, CancellationToken ct = default);
}

