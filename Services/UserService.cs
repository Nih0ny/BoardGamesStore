using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class UserService(ApplicationDbContext context) : IUserService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
  {
    var query = _context.Users.OrderByDescending(u => u.CreatedAt);
    var totalCount = await query.CountAsync(ct);

    var users = await query
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .Select(u => new UserDto
      {
        Id = u.Id,
        Email = u.Email,
        UserName = u.UserName,
        BonusBalance = u.Coins
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
      .Where(u => u.Id == userId)
      .Select(u => new UserDto
      {
        Id = u.Id,
        Email = u.Email,
        UserName = u.UserName,
        BonusBalance = u.Coins
      })
      .FirstOrDefaultAsync(ct);

    return user;
  }
}