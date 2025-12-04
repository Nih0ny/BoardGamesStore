using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BoardGamesStore.Models.Entities;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api")]
public class CommentsController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    // Отримати всі коментарі для продукту
    // GET: /api/products/{productId}/comments
    [HttpGet("products/{productId:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForProduct(int productId)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == productId))
        {
            return NotFound($"Product with id {productId} not found.");
        }

        var comments = await _context.Comments
            .Where(c => c.ProductId == productId)
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
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments);
    }

    // Отримати всі коментарі
    // GET: /api/comments
    [HttpGet("comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
    {
        var comments = await _context.Comments
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
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments);
    }

    // Отримати всі коментарі користувача
    // GET: /api/comments/user/{userId}
    [HttpGet("comments/user/{userId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByUser(string userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId))
        {
            return NotFound($"User with id {userId} not found.");
        }

        var comments = await _context.Comments
            .Where(c => c.UserId == userId)
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
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments);
    }

    // Створити новий коментар
    // POST: /api/products/{productId}/comments
    [HttpPost("products/{productId:int}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> AddComment(int productId, [FromBody] CreateCommentDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound($"Product with id {productId} not found.");
        }

        var comment = new Comment
        {
            Content = createDto.Content,
            ProductId = productId,
            Product = product,
            UserId = userId,
            User = null!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Повертаємо повний DTO з інформацією про користувача
        var user = await _context.Users.FindAsync(userId);
        var resultDto = new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            UserId = comment.UserId,
            UserName = user?.UserName ?? "Anonymous",
            ProductId = comment.ProductId
        };

        return CreatedAtAction(nameof(GetCommentsForProduct), new { productId = comment.ProductId }, resultDto);
    }

    // Оновити існуючий коментар
    // PUT: /api/comments/{commentId}
    [HttpPut("comments/{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
        {
            return NotFound();
        }

        // Перевірка: коментар належить користувачу АБО користувач є адміном
        if (comment.UserId != userId && !isAdmin)
        {
            return Forbid(); // 403 Forbidden - користувач аутентифікований, але не має прав
        }

        comment.Content = updateDto.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        _context.Entry(comment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Comments.Any(e => e.Id == commentId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // 204 No Content - успішне оновлення
    }

    // Видалити коментар
    // DELETE: /api/comments/{commentId}
    [HttpDelete("comments/{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return NotFound();
        }

        // Перевірка: коментар належить користувачу АБО користувач є адміном
        if (comment.UserId != userId && !isAdmin)
        {
            return Forbid();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content - успішне видалення
    }
}

