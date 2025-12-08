using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Services;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services;

public class ProductReportService(ApplicationDbContext context) //: IProductReportService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PagedResult<ProductReportDto>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
  {
    var query = _context.ProductReports.AsNoTracking();

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderByDescending(r => r.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(pr => new ProductReportDto
        {
          ProductId = pr.ProductId,
          ProductName = pr.Product.Name,
          ReportedBy = new UserDto
          {
            Id = pr.UserId,
            UserName = pr.User.UserName,
            Email = pr.User.Email,
          },
          Reason = pr.Reason
        })
        .ToListAsync(ct);

    return new PagedResult<ProductReportDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<ProductReportDto?> GetByIdAsync(
      int id,
      CancellationToken ct = default)
  {
    return await _context.ProductReports
        .AsNoTracking()
        .Where(pr => pr.Id == id)
        .OrderByDescending(r => r.CreatedAt)
        .Select(pr => new ProductReportDto
        {
          ProductId = pr.ProductId,
          ProductName = pr.Product.Name,
          ReportedBy = new UserDto
          {
            Id = pr.UserId,
            UserName = pr.User.UserName,
            Email = pr.User.Email,
          },
          Reason = pr.Reason
        })
        .FirstOrDefaultAsync(ct);
  }

  public async Task<Result<ProductReportDto>> CreateAsync(string userId, int productId, string reason, string status, CancellationToken ct = default)
  {
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null) return Result.Fail<ProductReportDto>($"User '{userId}' not found.");

    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null) return Result.Fail<ProductReportDto>($"Product '{productId}' not found.");

    var report = new ProductReport
    {
      UserId = userId,
      User = user,
      ProductId = productId,
      Product = product,
      Reason = reason,
      Status = status,
      CreatedAt = DateTime.UtcNow
    };

    _context.ProductReports.Add(report);
    await _context.SaveChangesAsync(ct);
    return Result.Ok(new ProductReportDto
    {
      ProductId = report.ProductId,
      ProductName = report.Product.Name,
      ReportedBy = new UserDto
      {
        Id = report.UserId,
        UserName = report.User.UserName,
        Email = report.User.Email,
      },
      Reason = report.Reason
    });
  }

  public async Task<Result> UpdateAsync(ProductReport report, CancellationToken ct = default)
  {
    if ((await ExistsAsync(report.Id, ct)).IsFailed) return Result.Fail("Product report not found.");

    // Optional: validate FKs if they can change
    var userExists = await _context.Users.AnyAsync(u => u.Id == report.UserId, ct);
    if (!userExists) return Result.Fail($"User '{report.UserId}' not found.");

    var productExists = await _context.Products.AnyAsync(p => p.Id == report.ProductId, ct);
    if (!productExists) return Result.Fail($"Product '{report.ProductId}' not found.");

    _context.ProductReports.Update(report);
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
  {
    var r = await _context.ProductReports.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (r is null) return Result.Fail("Product report not found.");

    _context.ProductReports.Remove(r);
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> ExistsAsync(int id, CancellationToken ct = default)
  {
    if (await _context.ProductReports.AnyAsync(x => x.Id == id, ct))
      return Result.Ok();
    else
      return Result.Fail("Product report not found.");
  }

  public async Task<Result> ChangeStatusAsync(int id, string status, CancellationToken ct = default)
  {
    var r = await _context.ProductReports.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (r is null) return Result.Fail("Product report not found.");

    r.Status = status;
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }
}
