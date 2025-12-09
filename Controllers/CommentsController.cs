using BoardGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BoardGamesStore.Interfaces;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
	private readonly ICommentService _commentService = commentService;

	[HttpGet("product/{productId:int}")]
	public async Task<ActionResult<PagedResult<CommentDto>>> GetCommentsForProduct(int productId, int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
	{
		try
		{
			var comments = await _commentService.GetForProductAsync(productId, pageNumber, pageSize, ct);
			return Ok(comments);
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
	}

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<PagedResult<CommentDto>>> GetAllComments(int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
	{
		var comments = await _commentService.GetAllAsync(pageNumber, pageSize, ct);
		return Ok(comments);
	}

	[HttpGet("user/{userId}")]
	public async Task<ActionResult<PagedResult<CommentDto>>> GetCommentsByUser(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
	{
		try
		{
			var comments = await _commentService.GetByUserIdAsync(userId, pageNumber, pageSize, ct);
			return Ok(comments);
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
	}

	[HttpPost("product/{productId:int}")]
	[Authorize]
	public async Task<ActionResult<CommentDto>> AddComment(int productId, [FromBody] CreateCommentDto createDto, CancellationToken ct = default)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId == null)
		{
			return Unauthorized();
		}

		try
		{
			var comment = await _commentService.CreateAsync(userId, productId, createDto.Content, ct);
			return CreatedAtAction(nameof(GetCommentsForProduct), new { productId }, comment);
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
	}

	[HttpPut("{commentId:int}")]
	[Authorize]
	public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto updateDto, CancellationToken ct = default)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var isAdmin = User.IsInRole("Admin");

		var comment = await _commentService.GetByIdAsync(commentId, ct);
		if (comment == null)
		{
			return NotFound();
		}

		// Перевірка: коментар належить користувачу АБО користувач є адміном
		if (comment.UserId != userId && !isAdmin)
		{
			return Forbid();
		}

		var success = await _commentService.UpdateAsync(commentId, updateDto.Content, ct);
		return success ? NoContent() : NotFound();
	}

	[HttpDelete("{commentId:int}")]
	[Authorize]
	public async Task<IActionResult> DeleteComment(int commentId, CancellationToken ct = default)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var isAdmin = User.IsInRole("Admin");

		var comment = await _commentService.GetByIdAsync(commentId, ct);
		if (comment == null)
		{
			return NotFound();
		}

		// Перевірка: коментар належить користувачу АБО користувач є адміном
		if (comment.UserId != userId && !isAdmin)
		{
			return Forbid();
		}

		var success = await _commentService.DeleteAsync(commentId, ct);
		return success ? NoContent() : NotFound();
	}
}

