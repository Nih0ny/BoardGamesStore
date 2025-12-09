using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using Sprache;
using BoardGamesStore.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/comment-reports")]
public class CommentReportsController(ICommentReportService commentReportService) : Controller
{
  private readonly ICommentReportService _commentReportService = commentReportService;

  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 100)
  {
    var result = await _commentReportService.GetAllAsync(pageNumber, pageSize);
    return Ok(result);
  }

  [HttpGet("comment/{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetReportsForComment(int id, int pageNumber = 1, int pageSize = 100, CancellationToken ct = default)
  {
    var result = await _commentReportService.GetAllAsync(pageNumber, pageSize, ct);
    return Ok(result);
  }

  [HttpGet("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetById(int id)
  {
    var r = await _commentReportService.GetByIdAsync(id);
    return r is null ? NotFound() : Ok(r);
  }

  [HttpGet("users/{userId}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetReportsForUser(string userId, CancellationToken ct = default)
  {
    return Ok(await _commentReportService.GetByUserIdAsync(userId, ct));
  }

  [HttpGet("my")]
  [Authorize]
  public async Task<IActionResult> GetUserReports(CancellationToken ct = default)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _commentReportService.GetByUserIdAsync(userId, ct);
    return Ok(result);
  }

  [HttpPost("{commentId:int}")]
  [Authorize]
  public async Task<IActionResult> Create(int commentId, [FromBody] CreateCommentReportDto dto, CancellationToken ct = default)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
      return Unauthorized();

    var result = await _commentReportService.CreateAsync(userId, commentId, dto.Reason ?? "", ct);
    if (result.IsFailed)
      return BadRequest(result.Errors.Select(e => e.Message));

    return CreatedAtAction(nameof(GetById), new { id = commentId }, result);
  }

  [HttpPatch("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Edit(int id, [FromBody] UpdateCommentReportDto dto, CancellationToken ct = default)
  {
    var commentReport = await _commentReportService.GetByIdAsync(id, ct);
    if (commentReport == null)
      return NotFound();

    var updatedDto = new UpdateCommentReportDto
    {
      UserId = commentReport.UserId,
      CommentId = dto.CommentId,
      Reason = dto.Reason
    };
    var result = await _commentReportService.UpdateAsync(updatedDto, ct);
    if (result.IsFailed)
      return BadRequest(result.Errors.Select(e => e.Message));

    return NoContent();
  }

  [HttpDelete("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
  {
    var result = await _commentReportService.DeleteAsync(id, ct);
    if (result.IsFailed)
      return BadRequest(result.Errors.Select(e => e.Message));

    return NoContent();
  }

  [HttpPatch("comments/reports/{id:int}/status")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeCommentReportStatusDto dto, CancellationToken ct = default)
  {
    var result = await _commentReportService.ChangeStatusAsync(id, dto.Status, ct);
    if (result.IsFailed)
      return BadRequest(result.Errors.Select(e => e.Message));

    return NoContent();
  }
}