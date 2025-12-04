using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services;

public class ProductService(ApplicationDbContext db) : IProductService
{
  private readonly ApplicationDbContext _db = db;

  public async Task<PagedResult<Product>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
  {
    // Починаємо будувати запит
    IQueryable<Product> query = _db.Products.AsNoTracking();

    // 1. Підвантажуємо SimilarProducts
    query = query.Include(p => p.SimilarProducts);

    // 2. Підвантажуємо лише 10 останніх коментарів (Filtered Include, доступно з EF Core 5+)
    // Припускаю, що у Comment є поле CreatedAt, інакше прибери OrderBy
    query = query.Include(p => p.Comments.OrderByDescending(c => c.CreatedAt).Take(10));

    // 3. Підвантажуємо дані з View (EvaluationSummary)
    // Тобі треба додати нову навігаційну властивість у Product, див. пояснення нижче
    query = query.Include(p => p.RatingSummary);

    // 4. Загальна кількість (для пагінації рахуємо ДО Skip/Take)
    var totalCount = await query.CountAsync(ct);

    // 5. Отримуємо дані сторінки
    var items = await query
        .OrderByDescending(p => p.CreatedAt) // Сортуємо (наприклад, нові зверху)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    // 6. Повертаємо результат
    return new PagedResult<Product>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
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

  // ---------- New query helpers ----------

  public async Task<PagedResult<Product>> SearchAsync(ProductQuery query, CancellationToken ct = default)
  {
    IQueryable<Product> q = _db.Products.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(query.Text))
    {
      var text = query.Text.Trim();
      q = q.Where(p =>
          EF.Functions.ILike(p.Name, $"%{text}%") ||
          (p.Description != null && EF.Functions.ILike(p.Description, $"%{text}%")));
    }

    if (!string.IsNullOrWhiteSpace(query.Category))
    {
      var cat = query.Category.Trim();
      q = q.Where(p => p.Category == cat);
    }

    if (query.MinPrice is not null) q = q.Where(p => p.Price >= query.MinPrice);
    if (query.MaxPrice is not null) q = q.Where(p => p.Price <= query.MaxPrice);
    if (query.InStockOnly) q = q.Where(p => p.Stock > 0);

    // Sorting
    q = (query.Sort?.ToLowerInvariant()) switch
    {
      "price_asc" => q.OrderBy(p => p.Price).ThenBy(p => p.Id),
      "price_desc" => q.OrderByDescending(p => p.Price).ThenByDescending(p => p.Id),
      "name" => q.OrderBy(p => p.Name).ThenBy(p => p.Id),
      "bonus_desc" => q.OrderByDescending(p => p.BonusRate).ThenByDescending(p => p.MaxBonusPaymentPercent),
      "newest" => q.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id),
      _ => q.OrderBy(p => p.Id)
    };

    var total = await q.CountAsync(ct);

    // Pagination
    var skip = Math.Max(0, query.Skip);
    var take = Math.Clamp(query.Take, 1, 200); // sane upper bound
    var items = await q.Skip(skip).Take(take).ToListAsync(ct);

    return new PagedResult<Product>
    {
      Items = items,
      TotalCount = total,
      PageNumber = (skip / take) + 1,
      PageSize = take
    };
  }

  public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
  {
    var idList = ids.Distinct().ToArray();
    if (idList.Length == 0) return new List<Product>();
    return await _db.Products.AsNoTracking()
        .Where(p => idList.Contains(p.Id))
        .ToListAsync(ct);
  }

  public async Task<List<Product>> GetSimilarAsync(int productId, int limit, CancellationToken ct = default)
  {
    // Use the navigation if SimilarProduct is a link entity (Product <-> Product).
    // Fallback: same category, excluding self.
    var baseProduct = await _db.Products.AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == productId, ct);

    if (baseProduct is null) return new List<Product>();

    // If you have a link table SimilarProduct with ProductId/SimilarToProductId,
    // you can query it here. Since we only have navs in the model, we’ll use category-based similarity.
    var q = _db.Products.AsNoTracking()
        .Where(p => p.Id != productId && p.Category == baseProduct.Category)
        .OrderByDescending(p => p.BonusRate)
        .ThenByDescending(p => p.CreatedAt);

    limit = Math.Clamp(limit, 1, 50);
    return await q.Take(limit).ToListAsync(ct);
  }

  public async Task<List<Product>> GetNewestAsync(int limit, string? category = null, CancellationToken ct = default)
  {
    IQueryable<Product> q = _db.Products.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(category))
      q = q.Where(p => p.Category == category);

    limit = Math.Clamp(limit, 1, 50);
    return await q.OrderByDescending(p => p.CreatedAt)
                  .ThenByDescending(p => p.Id)
                  .Take(limit)
                  .ToListAsync(ct);
  }

  public async Task<List<CategoryCount>> GetCategoriesWithCountsAsync(CancellationToken ct = default)
  {
    return await _db.Products.AsNoTracking()
        .GroupBy(p => p.Category ?? "(uncategorized)")
        .Select(g => new CategoryCount(g.Key, g.Count()))
        .OrderByDescending(cc => cc.Count)
        .ThenBy(cc => cc.Category)
        .ToListAsync(ct);
  }

  public async Task<(decimal min, decimal max, int count)> GetPriceStatsAsync(string? category = null, CancellationToken ct = default)
  {
    IQueryable<Product> q = _db.Products.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(category))
      q = q.Where(p => p.Category == category);

    var count = await q.CountAsync(ct);
    if (count == 0) return (0m, 0m, 0);

    var min = await q.MinAsync(p => p.Price, ct);
    var max = await q.MaxAsync(p => p.Price, ct);
    return (min, max, count);
  }

  public async Task<bool> SetDiscountAsync(int productId, decimal discountPercentage, CancellationToken ct = default)
  {
    var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId, ct);
    if (p is null) return false;

    p.MaxBonusPaymentPercent = discountPercentage;
    p.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync(ct);
    return true;
  }
}
