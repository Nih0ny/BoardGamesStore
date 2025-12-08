using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluationController(IEvaluationService evaluationService) : Controller
{
  private readonly IEvaluationService _evaluationService = evaluationService;

  // [Authorize]
  // [HttpPut("{productId}/evaluation")]
  // public async Task<IActionResult> RateProduct(int productId, [FromBody] RateProductDto dto)
  // {
  //   // dto.Score валідується автоматично через [Range(1,5)]
  //   var userId = GetCurrentUserId();
  //   var result = await _service.UpsertEvaluationAsync(productId, userId, dto.Score);
  //   // ... обробка результату
  //   return Ok();
  // }

  // // DELETE: api/products/10/evaluation
  // [HttpDelete("{productId}/evaluation")]
  // public async Task<IActionResult> RemoveRating(int productId)
  // {
  //   var userId = GetCurrentUserId();
  //   var result = await _service.RemoveEvaluationAsync(productId, userId);
  //   // ... обробка результату
  //   return NoContent();
  // }
}

