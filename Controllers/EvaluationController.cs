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

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluationController : Controller
{
  [Authorize]
  [HttpPut("{productId:int}")]
  public async Task<IActionResult> EvaluateProduct(int productId, [FromBody] int score)
  {
    // Логіка оцінки продукту користувачем
    return Ok();
  }
}

