using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
using BoardGamesStore.Services;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BoardGamesStore.Services;

public class ProductReportService(ApplicationDbContext context) : IProductReportService
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
            UserName = pr.User!.UserName,
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
        .Select(pr => new ProductReportDto
        {
          ProductId = pr.ProductId,
          ProductName = pr.Product.Name,
          ReportedBy = new UserDto
          {
            Id = pr.UserId,
            UserName = pr.User!.UserName,
            Email = pr.User.Email,
          },
          Reason = pr.Reason
        })
        .FirstOrDefaultAsync(ct);
  }

  public async Task<Result<ProductReportDto>> CreateAsync(string userId, int productId, string reason, CancellationToken ct = default)
  {
    try
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
      if (user == null) return Result.Fail<ProductReportDto>($"User '{userId}' not found.");

      var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
      if (product == null) return Result.Fail<ProductReportDto>($"Product '{productId}' not found.");

      var status = await _context.ReportStatuses.FirstOrDefaultAsync(s => s.Id == ReportStatusId.Pending, ct);
      if (status == null) return Result.Fail<ProductReportDto>("Status 'Pending' not found.");

      var report = new ProductReport
      {
        UserId = userId,
        User = user,
        ProductId = productId,
        Product = product,
        Reason = reason,
        StatusId = ReportStatusId.Pending,
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
    catch (Exception ex)
    {
      return Result.Fail<ProductReportDto>($"Error creating product report: {ex.Message}");
    }
  }

  public async Task<Result> UpdateAsync(ProductReport report, CancellationToken ct = default)
  {
    try
    {
      var existing = await _context.ProductReports.FirstOrDefaultAsync(x => x.Id == report.Id, ct);
      if (existing == null) return Result.Fail("Product report not found.");

      var userExists = await _context.Users.AnyAsync(u => u.Id == report.UserId, ct);
      if (!userExists) return Result.Fail($"User '{report.UserId}' not found.");

      var productExists = await _context.Products.AnyAsync(p => p.Id == report.ProductId, ct);
      if (!productExists) return Result.Fail($"Product '{report.ProductId}' not found.");

      _context.ProductReports.Update(report);
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error updating product report: {ex.Message}");
    }
  }

  public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
  {
    try
    {
      var r = await _context.ProductReports.FirstOrDefaultAsync(x => x.Id == id, ct);
      if (r is null) return Result.Fail("Product report not found.");

      _context.ProductReports.Remove(r);
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error deleting product report: {ex.Message}");
    }
  }

  public async Task<Result> ExistsAsync(int id, CancellationToken ct = default)
  {
    try
    {
      var exists = await _context.ProductReports.AnyAsync(x => x.Id == id, ct);
      return exists ? Result.Ok() : Result.Fail("Product report not found.");
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error checking product report existence: {ex.Message}");
    }
  }

  public async Task<Result> ChangeStatusAsync(int id, ReportStatusId status, CancellationToken ct = default)
  {
    try
    {
      var r = await _context.ProductReports.FirstOrDefaultAsync(x => x.Id == id, ct);
      if (r is null) return Result.Fail("Product report not found.");

      var reportStatus = await _context.ReportStatuses.FirstOrDefaultAsync(s => s.Id == status, ct);
      if (reportStatus == null) return Result.Fail("Report status not found.");

      r.StatusId = status;
      r.Status = reportStatus;
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error changing report status: {ex.Message}");
    }
  }

  public async Task<Result<ReportStatusId>> GetStatusAsync(int id, CancellationToken ct = default)
  {
    try
    {
      var reportStatus = await _context.ProductReports
          .AsNoTracking()
          .Where(x => x.Id == id)
          .Select(x => x.StatusId)
          .FirstOrDefaultAsync(ct);

      if (reportStatus == default) return Result.Fail<ReportStatusId>("Product report not found.");
      return Result.Ok(reportStatus);
    }
    catch (Exception ex)
    {
      return Result.Fail<ReportStatusId>($"Error getting report status: {ex.Message}");
    }
  }
}
