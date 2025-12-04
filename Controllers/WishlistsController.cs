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

namespace BoardGamesStore.Controllers;

public class WishlistsController(IWishlistService wishlistService) : Controller
{
  private readonly IWishlistService _wishlistService = wishlistService;
  // GET By User Id or Email (using query), return json with products in wishlist

  [HttpGet]
  [Authorize]
  public async Task<IActionResult> GetUserWishlist()
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var wishlist = await _wishlistService.GetByUserIdAsync(userId);
    if (wishlist == null) return NotFound();

    return Ok(wishlist);
  }

  // GET All Wishlists (only for admin) (with pagination return 10 users with 10 products and with posibility request new 10 users or 10 products for user))
  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAllWishlists(int userPage = 1, int productPage = 1)
  {
    var wishlists = await _wishlistService.GetAllAsync(userPage, productPage);
    return Ok(wishlists);
  }

  // POST Add new product in wishlist
  [Authorize]
  [HttpPost("add")]
  public async Task<IActionResult> AddProductToWishlist([FromBody] int productId)
  {
    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    if (string.IsNullOrEmpty(email)) return Unauthorized();

    var wishlistItem = await _wishlistService.AddAsync(email, productId);
    return Ok(wishlistItem);
  }

  // DELETE Remove product from wishlist
  [Authorize]
  [HttpDelete("remove")]
  public async Task<IActionResult> RemoveProductFromWishlist(int id)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var wishlist = await _wishlistService.GetByUserIdAsync(userId);
    if (wishlist == null) return NotFound();
    if (wishlist.UserId != userId && !User.IsInRole("Admin")) return Forbid();
    var result = await _wishlistService.DeleteAsync(id);
    if (result.IsFailed) return NotFound();
    return NoContent();
  }
}
