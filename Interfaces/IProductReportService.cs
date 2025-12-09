using BoardGamesStore.Models;
using BoardGamesStore.Models.Enums;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IProductReportService
{
  Task<PagedResult<ProductReportDto>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);

  Task<ProductReportDto?> GetByIdAsync(
    int id,
    CancellationToken ct = default);

  Task<Result<ProductReportDto>> CreateAsync(string userId, int productId, string reason, CancellationToken ct = default);

  Task<Result> UpdateAsync(int id, UpdateProductReportDto dto, CancellationToken ct = default);

  Task<Result> DeleteAsync(int id, CancellationToken ct = default);

  Task<Result> ExistsAsync(int id, CancellationToken ct = default);

  Task<Result> ChangeStatusAsync(int id, ReportStatusId status, CancellationToken ct = default);

  Task<Result<ReportStatusId>> GetStatusAsync(int id, CancellationToken ct = default);
}

