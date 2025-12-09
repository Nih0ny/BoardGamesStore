using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Models.Enums;
using BoardGamesStore.Interfaces;
using System.Security.Claims;

namespace BoardGamesStore.Controllers;

[Route("api/product-reports")]
[ApiController]
public class ProductReportsController(IProductReportService productReportService) : ControllerBase
{
  private readonly IProductReportService _productReportService = productReportService;

  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
  {
    var result = await _productReportService.GetAllAsync(pageNumber, pageSize, ct);
    return Ok(result);
  }

  [HttpGet("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
  {
    var report = await _productReportService.GetByIdAsync(id, ct);
    return report is null ? NotFound() : Ok(report);
  }

  [HttpGet("my")]
  [Authorize]
  public async Task<IActionResult> GetMyReports([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
  {
    var userId = GetCurrentUserId();
    if (userId is null) return Unauthorized();

    var result = await _productReportService.GetAllAsync(pageNumber, pageSize, ct);
    return Ok(result);
  }

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

  [HttpPatch("{id:int}/reason")]
  [Authorize]
  public async Task<IActionResult> UpdateReason(int id, [FromBody] UpdateProductReportDto dto, CancellationToken ct = default)
  {
    var existing = await _productReportService.GetByIdAsync(id, ct);
    if (existing is null) return NotFound();
    if (existing.ReportedBy?.Id != GetCurrentUserId()) return Forbid();

    var result = await _productReportService.UpdateAsync(id, dto, ct);
    return result.IsSuccess ? NoContent() : BadRequest(result.Errors.Select(e => e.Message));
  }

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

  [HttpPatch("{id:int}/status")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> UpdateStatus(int id, [FromBody] ChangeProductReportStatusDto dto, CancellationToken ct = default)
  {
    if (!Enum.TryParse<ReportStatusId>(dto.Status, true, out var statusId))
      return BadRequest($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames(typeof(ReportStatusId)))}");

    var result = await _productReportService.ChangeStatusAsync(id, statusId, ct);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  private string? GetCurrentUserId()
  {
    return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  }
}