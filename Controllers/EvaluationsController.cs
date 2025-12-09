using System.Security.Claims;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/products/{productId:int}/evaluations")]
public class EvaluationsController(IEvaluationService evaluationService) : ControllerBase
{
  private readonly IEvaluationService _evaluationService = evaluationService;

  private string? GetCurrentUserId()
  {
    return User.FindFirstValue(ClaimTypes.NameIdentifier);
  }

  [Authorize]
  [HttpPut]
  public async Task<IActionResult> RateProduct(int productId, [FromBody] RateProductDto dto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var result = await _evaluationService.UpsertEvaluationAsync(productId, userId, dto.Score);

    if (!result.IsSuccess)
    {
      return BadRequest(result.Errors);
    }

    return Ok(new { message = "Rating saved successfully" });
  }

  [Authorize]
  [HttpDelete]
  public async Task<IActionResult> RemoveRating(int productId)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var result = await _evaluationService.RemoveEvaluationAsync(productId, userId);

    if (!result.IsSuccess)
    {
      return BadRequest(result.Errors);
    }

    return NoContent();
  }
}