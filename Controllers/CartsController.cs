using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using BoardGamesStore.Interfaces;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/carts")]
public class CartsController(ICartService cartService) : ControllerBase
{
  private readonly ICartService _cartService = cartService;

  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAll([FromQuery] int userPage = 1, [FromQuery] int itemPage = 1)
  {
    return Ok(await _cartService.GetUsersWithCartsAsync(userPage, itemPage));
  }

  [Authorize]
  [HttpGet("my")]
  public async Task<IActionResult> GetUserCart()
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var cart = await _cartService.GetByUserIdAsync(userId);
    return Ok(cart);
  }

  [HttpGet("{userId}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetByUserId(string userId)
  {
    var cart = await _cartService.GetByUserIdAsync(userId);
    if (cart is null) return NotFound();
    return Ok(new { cart });
  }

  [Authorize]
  [HttpPost("items")]
  public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _cartService.AddItemAsync(userId, dto.ProductId, dto.Quantity);
    if (result.IsFailed) return BadRequest(result.Errors);

    // Only return product name and basic info, not full entity with cycles
    return Created();
  }

  [Authorize]
  [HttpDelete("items/{productId:int}")]
  public async Task<IActionResult> RemoveItem(int productId)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _cartService.RemoveItemAsync(userId, productId);
    if (result.IsFailed) return NotFound();

    return NoContent();
  }

  [Authorize]
  [HttpPatch("items/{productId:int}")]
  public async Task<IActionResult> UpdateItemQuantity(int productId, [FromBody] CartItemQuantityUpdateDto dto)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _cartService.UpdateItemQuantityAsync(userId, productId, dto.Quantity);
    if (result.IsFailed) return NotFound();

    return NoContent();
  }
}