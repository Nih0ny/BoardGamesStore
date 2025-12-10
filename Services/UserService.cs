using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class UserService(ApplicationDbContext context, IWebHostEnvironment environment) : IUserService
{
  private readonly ApplicationDbContext _context = context;
  private readonly IWebHostEnvironment _environment = environment;
  private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
  private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

  public async Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
  {
    var query = _context.Users.Where(u => !u.IsDeleted).OrderByDescending(u => u.CreatedAt);
    var totalCount = await query.CountAsync(ct);

    var users = await query
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .Select(u => new UserDto
      {
        Id = u.Id,
        Email = u.Email,
        UserName = u.UserName,
        BonusBalance = u.Coins,
        AvatarUrl = u.AvatarUrl
      })
      .ToListAsync(ct);

    return new PagedResult<UserDto>
    {
      Items = users,
      TotalCount = totalCount,
      PageNumber = pageNumber,
      PageSize = pageSize
    };
  }

  public async Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken ct = default)
  {
    var user = await _context.Users
      .AsNoTracking()
      .Where(u => u.Id == userId && !u.IsDeleted)
      .Select(u => new UserDto
      {
        Id = u.Id,
        Email = u.Email,
        UserName = u.UserName,
        BonusBalance = u.Coins,
        AvatarUrl = u.AvatarUrl
      })
      .FirstOrDefaultAsync(ct);

    return user;
  }

  public async Task<Result> UploadAvatarAsync(string userId, IFormFile file, CancellationToken ct = default)
  {
    if (file == null || file.Length == 0)
      return Result.Fail("File is required.");

    if (file.Length > MaxFileSize)
      return Result.Fail($"File size must not exceed {MaxFileSize / (1024 * 1024)} MB.");

    var fileExtension = Path.GetExtension(file.FileName).ToLower();
    if (!_allowedExtensions.Contains(fileExtension))
      return Result.Fail($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null)
      return Result.Fail("User not found.");

    try
    {
      // Delete old avatar if exists
      if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
      {
        var oldFilePath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
        if (File.Exists(oldFilePath))
        {
          File.Delete(oldFilePath);
        }
      }

      // Create avatars directory if it doesn't exist
      var avatarDirectory = Path.Combine(_environment.WebRootPath, "images", "avatars");
      if (!Directory.Exists(avatarDirectory))
      {
        Directory.CreateDirectory(avatarDirectory);
      }

      // Generate unique filename
      var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{fileExtension}";
      var filePath = Path.Combine(avatarDirectory, fileName);

      // Save file
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await file.CopyToAsync(stream, ct);
      }

      // Update user avatar URL
      user.AvatarUrl = $"/images/avatars/{fileName}";
      user.UpdatedAt = DateTime.UtcNow;
      _context.Users.Update(user);
      await _context.SaveChangesAsync(ct);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error uploading avatar: {ex.Message}");
    }
  }

  public async Task<Result> DeleteAvatarAsync(string userId, CancellationToken ct = default)
  {
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null)
      return Result.Fail("User not found.");

    if (string.IsNullOrWhiteSpace(user.AvatarUrl))
      return Result.Fail("User has no avatar.");

    try
    {
      var filePath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
      if (File.Exists(filePath))
      {
        File.Delete(filePath);
      }

      user.AvatarUrl = null;
      user.UpdatedAt = DateTime.UtcNow;
      _context.Users.Update(user);
      await _context.SaveChangesAsync(ct);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Error deleting avatar: {ex.Message}");
    }
  }
}