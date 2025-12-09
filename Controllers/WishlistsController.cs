using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.AspNetCore.Authorization;
using BoardGamesStore.Interfaces;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/wishlists")]
public class WishlistsController(IWishlistService wishlistService) : ControllerBase
{
  private readonly IWishlistService _wishlistService = wishlistService;

  [HttpGet("my")]
  [Authorize]
  public async Task<IActionResult> GetUserWishlist()
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var wishlist = await _wishlistService.GetByUserIdAsync(userId);

    return Ok(wishlist);
  }

  [HttpGet("user/{userId}")]
  public async Task<IActionResult> GetWishlistByUserId(string userId)
  {
    var wishlist = await _wishlistService.GetByUserIdAsync(userId);
    return Ok(wishlist);
  }

  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAllWishlists(int userPage = 1, int productPage = 1)
  {
    var wishlists = await _wishlistService.GetAllAsync(userPage, productPage);
    return Ok(wishlists);
  }

  [Authorize]
  [HttpPost]
  public async Task<IActionResult> AddProductToWishlist([FromBody] CreateWishlistItemDto dto)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _wishlistService.AddAsync(userId, dto.ProductId);
    if (result.IsFailed) return BadRequest(result.Errors);

    return Ok(result.Value);
  }

  [Authorize]
  [HttpDelete("{productId:int}")]
  public async Task<IActionResult> RemoveProductFromWishlist(int productId)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _wishlistService.DeleteAsync(userId, productId);
    if (result.IsFailed) return NotFound(result.Errors);
    return NoContent();
  }
}
