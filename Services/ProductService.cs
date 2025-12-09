using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class ProductService(ApplicationDbContext context) : IProductService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchQuery query, CancellationToken ct = default)
  {
    var dbQuery = _context.Products
        .AsNoTracking()
        .Include(p => p.Categories)
        .ThenInclude(pc => pc.Category)
        .Include(p => p.RatingSummary)
        .Where(p => !p.IsDeleted)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var term = query.SearchText.Trim();
      dbQuery = dbQuery.Where(p =>
          p.Name.Contains(term) ||
          (p.Description != null && p.Description.Contains(term)));
    }

    if (!string.IsNullOrWhiteSpace(query.Category))
    {
      dbQuery = dbQuery.Where(p => p.Categories.Any(pc => pc.Category.Name == query.Category));
    }

    if (query.MinPrice.HasValue)
    {
      dbQuery = dbQuery.Where(p => p.Price >= query.MinPrice.Value);
    }

    if (query.MaxPrice.HasValue)
    {
      dbQuery = dbQuery.Where(p => p.Price <= query.MaxPrice.Value);
    }

    if (query.InStock.HasValue && query.InStock.Value)
    {
      dbQuery = dbQuery.Where(p => p.Stock > 0);
    }

    dbQuery = query.SortBy?.ToLower() switch
    {
      "price_asc" => dbQuery.OrderBy(p => p.Price),
      "price_desc" => dbQuery.OrderByDescending(p => p.Price),
      "name" => dbQuery.OrderBy(p => p.Name),
      "newest" => dbQuery.OrderByDescending(p => p.CreatedAt),
      _ => dbQuery.OrderByDescending(p => p.CreatedAt)
    };

    var totalCount = await dbQuery.CountAsync(ct);

    var items = await dbQuery
        .Skip((query.PageNumber - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync(ct);

    var dtos = items.Select(MapProductToDto).ToList();

    return new PagedResult<ProductDto>
    {
      Items = dtos,
      TotalCount = totalCount,
      PageNumber = query.PageNumber,
      PageSize = query.PageSize
    };
  }

  public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
  {
    var product = await _context.Products
        .AsNoTracking()
        .Include(p => p.Categories)
        .ThenInclude(pc => pc.Category)
        .Include(p => p.RatingSummary)
        .Where(p => p.Id == id && !p.IsDeleted)
        .FirstOrDefaultAsync(ct);

    return product == null ? null : MapProductToDto(product);
  }

  public async Task<List<CategoryCount>> GetCategoryStatsAsync(CancellationToken ct = default)
  {
    return await _context.ProductCategories
        .AsNoTracking()
        .GroupBy(pc => pc.Category.Name)
        .Select(g => new CategoryCount { Category = g.Key, Count = g.Count() })
        .OrderByDescending(cc => cc.Count)
        .ThenBy(cc => cc.Category)
        .ToListAsync(ct);
  }

  public async Task<(decimal min, decimal max)> GetPriceRangeAsync(string? category = null, CancellationToken ct = default)
  {
    IQueryable<Product> query = _context.Products
        .AsNoTracking()
        .Where(p => !p.IsDeleted);

    if (!string.IsNullOrWhiteSpace(category))
    {
      query = query.Where(p => p.Categories.Any(pc => pc.Category.Name == category));
    }

    var count = await query.CountAsync(ct);
    if (count == 0) return (0m, 0m);

    var min = await query.MinAsync(p => p.Price, ct);
    var max = await query.MaxAsync(p => p.Price, ct);
    return (min, max);
  }

  public async Task<List<ProductDto>> GetSimilarAsync(int productId, int limit = 5, CancellationToken ct = default)
  {
    var baseProduct = await _context.Products
        .AsNoTracking()
        .Include(p => p.Categories)
        .ThenInclude(pc => pc.Category)
        .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, ct);

    if (baseProduct == null)
      return [];

    var baseCategory = baseProduct.Categories.FirstOrDefault()?.CategoryId;

    var query = _context.Products
        .AsNoTracking()
        .Include(p => p.Categories)
        .ThenInclude(pc => pc.Category)
        .Include(p => p.RatingSummary)
        .Where(p => p.Id != productId && !p.IsDeleted);

    if (baseCategory.HasValue)
    {
      query = query.Where(p => p.Categories.Any(pc => pc.CategoryId == baseCategory));
    }

    limit = Math.Clamp(limit, 1, 50);

    var results = await query
        .OrderByDescending(p => p.RatingSummary != null ? p.RatingSummary.AverageRating : 0)
        .ThenByDescending(p => p.CreatedAt)
        .Take(limit)
        .ToListAsync(ct);

    return [.. results.Select(MapProductToDto)];
  }

  public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
  {
    var product = new Product
    {
      Name = dto.Name,
      Description = dto.Description,
      Price = dto.Price,
      BonusRate = 0.5m,
      MaxBonusPaymentPercent = 100m,
      Stock = dto.StockQuantity,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
      IsDeleted = false
    };

    _context.Products.Add(product);
    await _context.SaveChangesAsync(ct);

    if (dto.Categories != null && dto.Categories.Count != 0)
    {
      var categories = await _context.Categories
          .Where(c => dto.Categories.Contains(c.Name))
          .ToListAsync(ct);

      foreach (var category in categories)
      {
        _context.ProductCategories.Add(new ProductCategory
        {
          ProductId = product.Id,
          CategoryId = category.Id
        });
      }

      await _context.SaveChangesAsync(ct);
    }

    return MapProductToDto(product);
  }

  public async Task<Result> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
  {
    var product = await _context.Products
        .Include(p => p.Categories)
        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

    if (product == null)
      return Result.Fail($"Product with ID {id} not found.");

    try
    {
      product.Name = dto.Name;
      product.Description = dto.Description;
      product.Price = dto.Price;
      product.UpdatedAt = DateTime.UtcNow;

      _context.Products.Update(product);

      if (dto.Categories != null)
      {
        _context.ProductCategories.RemoveRange(product.Categories);

        var categories = await _context.Categories
            .Where(c => dto.Categories.Contains(c.Name))
            .ToListAsync(ct);

        foreach (var category in categories)
        {
          _context.ProductCategories.Add(new ProductCategory
          {
            ProductId = product.Id,
            CategoryId = category.Id
          });
        }
      }

      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error updating product: {ex.Message}");
    }
  }

  public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    if (product == null)
      return Result.Fail($"Product with ID {id} not found.");

    try
    {
      product.IsDeleted = true;
      product.UpdatedAt = DateTime.UtcNow;
      _context.Products.Update(product);
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error deleting product: {ex.Message}");
    }
  }

  public async Task<Result> AdjustStockAsync(int productId, int quantityChange, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      return Result.Fail($"Product with ID {productId} not found.");

    if (product.Stock + quantityChange < 0)
      return Result.Fail("Insufficient stock.");

    try
    {
      checked { product.Stock += quantityChange; }
      product.UpdatedAt = DateTime.UtcNow;
      _context.Products.Update(product);
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (OverflowException)
    {
      return Result.Fail("Stock adjustment would cause overflow.");
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error adjusting stock: {ex.Message}");
    }
  }

  public async Task<Result> SetDiscountAsync(int productId, SetProductDiscountDto dto, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      return Result.Fail($"Product with ID {productId} not found.");

    if (dto.DiscountPercent.HasValue)
    {
      if (dto.DiscountPercent < 0 || dto.DiscountPercent > 1)
        return Result.Fail("Discount must be between 0 and 1 percent.");
    }

    if (dto.DiscountPercent != 0)
    {
      // FIXME: Notify User
    }

    try
    {
      product.DiscountPercent = dto.DiscountPercent;
      product.UpdatedAt = DateTime.UtcNow;
      _context.Products.Update(product);
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error setting discount: {ex.Message}");
    }
  }

  private ProductDto MapProductToDto(Product product)
  {
    return new ProductDto
    {
      Id = product.Id,
      Name = product.Name,
      Price = product.Price,
      BonusRate = product.BonusRate,
      MaxBonusPaymentPercent = product.MaxBonusPaymentPercent,
      Description = product.Description,
      Rating = product.RatingSummary?.AverageRating,
      IsInStock = product.Stock > 0,
      Categories = product.Categories?.Select(pc => pc.Category.Name).ToList() ?? [],
      ImageUrl = string.IsNullOrWhiteSpace(product.ImageUrl) ? null : new List<string> { product.ImageUrl }
    };
  }
}
