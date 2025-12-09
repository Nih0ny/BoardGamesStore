using BoardGamesStore.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoardGamesStore.Interfaces;

public interface ICommentService
{
  Task<PagedResult<CommentDto>> GetAllAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);
  Task<PagedResult<CommentDto>> GetForProductAsync(int productId, int pageNumber, int pageSize, CancellationToken ct = default);
  Task<CommentDto?> GetByIdAsync(int commentId, CancellationToken ct = default);
  Task<PagedResult<CommentDto>> GetByUserIdAsync(string userId, int pageNumber, int pageSize, CancellationToken ct = default);
  Task<CommentDto> CreateAsync(string userId, int productId, string content, CancellationToken ct = default);
  Task<bool> UpdateAsync(int commentId, string content, CancellationToken ct = default);
  Task<bool> DeleteAsync(int commentId, CancellationToken ct = default);
}