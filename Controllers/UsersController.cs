using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(users);
    }

    [HttpPost("avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _userService.UploadAvatarAsync(userId, file, ct);
        return result.IsSuccess ? Ok(new { Message = "Avatar uploaded successfully." }) : BadRequest(result.Errors.Select(e => e.Message));
    }

    [HttpPatch("avatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar(IFormFile file, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _userService.UploadAvatarAsync(userId, file, ct);
        return result.IsSuccess ? Ok(new { Message = "Avatar updated successfully." }) : BadRequest(result.Errors.Select(e => e.Message));
    }

    [HttpDelete("avatar")]
    [Authorize]
    public async Task<IActionResult> DeleteAvatar(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _userService.DeleteAvatarAsync(userId, ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Errors.Select(e => e.Message));
    }

    [HttpGet("{userId}/avatar")]
    public async Task<IActionResult> GetUserAvatar(string userId, CancellationToken ct)
    {
        var user = await _userService.GetUserByIdAsync(userId, ct);
        if (user == null)
            return NotFound();

        return Ok(new { user.AvatarUrl });
    }

    [HttpGet("my/avatar")]
    [Authorize]
    public async Task<IActionResult> GetMyAvatar(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId, ct);
        if (user == null)
            return NotFound();

        return Ok(new { user.AvatarUrl });
    }
}