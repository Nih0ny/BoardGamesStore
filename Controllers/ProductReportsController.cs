using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
using BoardGamesStore.Interfaces;
using System.Security.Claims;

namespace BoardGamesStore.Controllers;

/// <summary>
/// API controller for managing product reports submitted by users for inappropriate or problematic products.
/// Allows users to submit reports and admins to manage, review, and update report status.
/// </summary>
[Route("api/product-reports")]
[ApiController]
public class ProductReportsController(IProductReportService productReportService) : ControllerBase
{
  private readonly IProductReportService _productReportService = productReportService;

  /// <summary>
  /// Retrieves all product reports with pagination. Admin only.
  /// </summary>
  /// <param name="pageNumber">Page number (default 1)</param>
  /// <param name="pageSize">Items per page (default 20)</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Paginated list of product reports</returns>
  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
  {
    var result = await _productReportService.GetAllAsync(pageNumber, pageSize, ct);
    return Ok(result);
  }

  /// <summary>
  /// Retrieves a specific product report by ID. Admin only.
  /// </summary>
  /// <param name="id">Report ID</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Product report details</returns>
  [HttpGet("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
  {
    var report = await _productReportService.GetByIdAsync(id, ct);
    return report is null ? NotFound() : Ok(report);
  }

  /// <summary>
  /// Retrieves product reports submitted by the current user with pagination.
  /// </summary>
  /// <param name="pageNumber">Page number (default 1)</param>
  /// <param name="pageSize">Items per page (default 20)</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Paginated list of user's product reports</returns>
  [HttpGet("my")]
  [Authorize]
  public async Task<IActionResult> GetMyReports([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
  {
    var userId = GetCurrentUserId();
    if (userId is null) return Unauthorized();

    var result = await _productReportService.GetAllAsync(pageNumber, pageSize, ct);
    return Ok(result);
  }

  /// <summary>
  /// Creates a new product report. Requires authentication.
  /// </summary>
  /// <param name="body">Report details including product ID and reason</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Created report details</returns>
  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Create([FromBody] CreateProductReportDto body, CancellationToken ct = default)
  {
    var userId = GetCurrentUserId();
    if (userId is null) return Unauthorized();

    var result = await _productReportService.CreateAsync(
        userId,
        body.ProductId,
        body.Reason ?? "",
        ct
    );

    if (result.IsFailed)
      return BadRequest(result.Errors.Select(e => e.Message));

    return CreatedAtAction(nameof(GetById), new { id = result.Value.ProductId }, result.Value);
  }

  // PUT api/product-reports/5
  // Прибрав "/update" з URL
  /// <summary>
  /// Updates an existing product report. User can only update their own reports.
  /// </summary>
  /// <param name="id">Report ID to update</param>
  /// <param name="dto">Updated report data</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>Updated report on success</returns>
  [HttpPut("{id:int}")]
  [Authorize]
  public async Task<IActionResult> Update(int id, [FromBody] ProductReport dto, CancellationToken ct = default)
  {
    if (id != dto.Id) return BadRequest("Mismatched id.");

    var existing = await _productReportService.GetByIdAsync(id, ct);
    if (existing is null) return NotFound();
    if (existing.ReportedBy?.Id != GetCurrentUserId()) return Forbid();

    var result = await _productReportService.UpdateAsync(dto, ct);
    return result.IsSuccess ? Ok(dto) : NotFound();
  }

  /// <summary>
  /// Deletes a product report. User can only delete their own reports.
  /// </summary>
  /// <param name="id">Report ID to delete</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>No content on success</returns>
  [HttpDelete("{id:int}")]
  [Authorize]
  public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
  {
    var existing = await _productReportService.GetByIdAsync(id, ct);
    if (existing is null) return NotFound();
    if (existing.ReportedBy?.Id != GetCurrentUserId()) return Forbid();

    var result = await _productReportService.DeleteAsync(id, ct);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  /// <summary>
  /// Changes the status of a product report. Admin only.
  /// </summary>
  /// <param name="id">Report ID</param>
  /// <param name="body">New status for the report</param>
  /// <param name="ct">Cancellation token</param>
  /// <returns>No content on success</returns>
  /// <summary>
  /// Updates the status of a product report. Admin only.
  /// </summary>
  [HttpPatch("{id:int}/status")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> UpdateStatus(int id, [FromBody] ChangeProductReportStatusDto dto, CancellationToken ct = default)
  {
    if (!Enum.TryParse<ReportStatusId>(dto.Status, true, out var statusId))
      return BadRequest($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames(typeof(ReportStatusId)))}");

    var result = await _productReportService.ChangeStatusAsync(id, statusId, ct);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  /// <summary>
  /// Helper method to get the current authenticated user's ID from claims.
  /// </summary>
  /// <returns>User ID or null if not authenticated</returns>
  private string? GetCurrentUserId()
  {
    return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  }
}