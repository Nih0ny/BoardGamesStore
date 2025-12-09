using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class CommentService(ApplicationDbContext context) : ICommentService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<CommentDto> CreateAsync(string userId, int productId, string content, CancellationToken ct = default)
  {
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null)
      throw new KeyNotFoundException($"User '{userId}' not found.");

    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      throw new KeyNotFoundException($"Product '{productId}' not found.");

    var comment = new Comment
    {
      UserId = userId,
      User = user,
      ProductId = productId,
      Product = product,
      Content = content,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    _context.Comments.Add(comment);
    await _context.SaveChangesAsync(ct);

    return new CommentDto
    {
      Id = comment.Id,
      Content = comment.Content!,
      CreatedAt = comment.CreatedAt,
      UpdatedAt = comment.UpdatedAt,
      UserId = comment.UserId,
      UserName = user.UserName ?? "Anonymous",
      ProductId = comment.ProductId
    };
  }

  public async Task<bool> DeleteAsync(int commentId, CancellationToken ct = default)
  {
    var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
    if (comment == null)
      return false;

    _context.Comments.Remove(comment);
    await _context.SaveChangesAsync(ct);
    return true;
  }

  public async Task<PagedResult<CommentDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
  {
    var query = _context.Comments
        .Include(c => c.User)
        .OrderByDescending(c => c.CreatedAt);

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CommentDto
        {
          Id = c.Id,
          Content = c.Content!,
          CreatedAt = c.CreatedAt,
          UpdatedAt = c.UpdatedAt,
          UserId = c.UserId,
          UserName = c.User!.UserName ?? "Anonymous",
          ProductId = c.ProductId
        })
        .ToListAsync(ct);

    return new PagedResult<CommentDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<CommentDto?> GetByIdAsync(int commentId, CancellationToken ct = default)
  {
    return await _context.Comments
        .AsNoTracking()
        .Where(c => c.Id == commentId)
        .Include(c => c.User)
        .Select(c => new CommentDto
        {
          Id = c.Id,
          Content = c.Content!,
          CreatedAt = c.CreatedAt,
          UpdatedAt = c.UpdatedAt,
          UserId = c.UserId,
          UserName = c.User!.UserName ?? "Anonymous",
          ProductId = c.ProductId
        })
        .FirstOrDefaultAsync(ct);
  }

  public async Task<PagedResult<CommentDto>> GetByUserIdAsync(string userId, int pageNumber, int pageSize, CancellationToken ct = default)
  {
    var query = _context.Comments
        .Where(c => c.UserId == userId)
        .Include(c => c.User)
        .OrderByDescending(c => c.CreatedAt);

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CommentDto
        {
          Id = c.Id,
          Content = c.Content!,
          CreatedAt = c.CreatedAt,
          UpdatedAt = c.UpdatedAt,
          UserId = c.UserId,
          UserName = c.User!.UserName ?? "Anonymous",
          ProductId = c.ProductId
        })
        .ToListAsync(ct);

    return new PagedResult<CommentDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<PagedResult<CommentDto>> GetForProductAsync(int productId, int pageNumber, int pageSize, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      throw new KeyNotFoundException($"Product '{productId}' not found.");

    var query = _context.Comments
        .Where(c => c.ProductId == productId)
        .Include(c => c.User)
        .OrderByDescending(c => c.CreatedAt);

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CommentDto
        {
          Id = c.Id,
          Content = c.Content!,
          CreatedAt = c.CreatedAt,
          UpdatedAt = c.UpdatedAt,
          UserId = c.UserId,
          UserName = c.User!.UserName ?? "Anonymous",
          ProductId = c.ProductId
        })
        .ToListAsync(ct);

    return new PagedResult<CommentDto>
    {
      Items = items,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<bool> UpdateAsync(int commentId, string content, CancellationToken ct = default)
  {
    var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
    if (comment == null)
      return false;

    comment.Content = content;
    comment.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync(ct);
    return true;
  }
}