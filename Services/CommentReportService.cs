using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class CommentReportService(ApplicationDbContext context) : ICommentReportService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PagedResult<CommentReportDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
  {
    var query = _context.CommentReports.AsNoTracking();

    var totalCount = await query.CountAsync(ct);

    var reports = await query
        .OrderByDescending(cr => cr.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    var dtos = reports.Select(MapCommentReportToDto).ToList();

    return new PagedResult<CommentReportDto>
    {
      Items = dtos,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<CommentReportDto?> GetByIdAsync(int id, CancellationToken ct = default)
  {
    var report = await _context.CommentReports
        .AsNoTracking()
        .FirstOrDefaultAsync(cr => cr.Id == id, ct);

    return report == null ? null : MapCommentReportToDto(report);
  }

  public async Task<List<CommentReportDto>> GetByUserIdAsync(string userId, CancellationToken ct = default)
  {
    var reports = await _context.CommentReports
        .AsNoTracking()
        .Where(cr => cr.UserId == userId)
        .OrderByDescending(cr => cr.CreatedAt)
        .ToListAsync(ct);

    return reports.Select(MapCommentReportToDto).ToList();
  }

  public async Task<Result> CreateAsync(string userId, int commentId, string? reason, CancellationToken ct = default)
  {
    try
    {
      var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
      if (comment == null)
        return Result.Fail($"Comment with ID {commentId} not found.");

      var existingReport = await _context.CommentReports
          .FirstOrDefaultAsync(cr => cr.UserId == userId && cr.CommentId == commentId, ct);
      if (existingReport != null)
        return Result.Fail("You have already reported this comment.");

      var defaultStatus = await _context.ReportStatuses
          .FirstOrDefaultAsync(s => s.Id == ReportStatusId.Pending, ct);
      if (defaultStatus == null)
        return Result.Fail("Report status not found.");

      var report = new CommentReport
      {
        UserId = userId,
        CommentId = commentId,
        Comment = comment,
        Reason = reason,
        StatusId = ReportStatusId.Pending,
        Status = defaultStatus,
        CreatedAt = DateTime.UtcNow
      };

      _context.CommentReports.Add(report);
      await _context.SaveChangesAsync(ct);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error creating comment report: {ex.Message}");
    }
  }

  public async Task<Result> UpdateAsync(int id, UpdateCommentReportDto dto, CancellationToken ct = default)
  {
    try
    {
      var existingReport = await _context.CommentReports.FirstOrDefaultAsync(cr => cr.Id == id, ct);
      if (existingReport == null)
        return Result.Fail("Comment report not found.");

      // Only allow update if status is Pending (not in review)
      if (existingReport.StatusId != ReportStatusId.Pending)
        return Result.Fail("Can only update reports with Pending status.");

      // Only allow updating reason
      if (!string.IsNullOrWhiteSpace(dto.Reason))
        existingReport.Reason = dto.Reason;

      _context.CommentReports.Update(existingReport);
      await _context.SaveChangesAsync(ct);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error updating comment report: {ex.Message}");
    }
  }

  public async Task<Result> ChangeStatusAsync(int commentReportId, ReportStatusId status, CancellationToken ct = default)
  {
    try
    {
      var report = await _context.CommentReports
          .FirstOrDefaultAsync(cr => cr.Id == commentReportId, ct);
      if (report == null)
        return Result.Fail($"Comment report with ID {commentReportId} not found.");

      var reportStatus = await _context.ReportStatuses
          .FirstOrDefaultAsync(s => s.Id == status, ct);
      if (reportStatus == null)
        return Result.Fail($"Report status not found.");

      report.StatusId = status;
      report.Status = reportStatus;

      _context.CommentReports.Update(report);
      await _context.SaveChangesAsync(ct);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error changing report status: {ex.Message}");
    }
  }

  public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
  {
    try
    {
      var report = await _context.CommentReports
          .FirstOrDefaultAsync(cr => cr.Id == id, ct);
      if (report == null)
        return Result.Fail($"Comment report with ID {id} not found.");

      _context.CommentReports.Remove(report);
      await _context.SaveChangesAsync(ct);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error deleting comment report: {ex.Message}");
    }
  }

  public async Task<bool> ExistsAsync(string userId, int commentId, CancellationToken ct = default)
  {
    return await _context.CommentReports
        .AnyAsync(cr => cr.UserId == userId && cr.CommentId == commentId, ct);
  }

  private CommentReportDto MapCommentReportToDto(CommentReport report)
  {
    return new CommentReportDto
    {
      Id = report.Id,
      UserId = report.UserId ?? string.Empty,
      CommentId = report.CommentId,
      Reason = report.Reason,
      StatusId = report.StatusId,
      CreatedAt = report.CreatedAt
    };
  }
}