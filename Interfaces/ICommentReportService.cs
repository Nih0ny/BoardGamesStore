using BoardGamesStore.Models;
using BoardGamesStore.Models.Enums;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface ICommentReportService
{
  Task<PagedResult<CommentReportDto>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);
  Task<CommentReportDto?> GetByIdAsync(int id, CancellationToken ct = default);
  Task<List<CommentReportDto>> GetByUserIdAsync(string userId, CancellationToken ct = default);
  Task<Result> CreateAsync(string userId, int commentId, string? reason, CancellationToken ct = default);
  Task<Result> UpdateAsync(int id, UpdateCommentReportDto dto, CancellationToken ct = default);
  Task<Result> ChangeStatusAsync(int commentReportId, ReportStatusId status, CancellationToken ct = default);
  Task<Result> DeleteAsync(int id, CancellationToken ct = default);
  Task<bool> ExistsAsync(string userId, int commentId, CancellationToken ct = default);
}