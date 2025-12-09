using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class BonusService(ApplicationDbContext context) : IBonusService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<Result<decimal>> GetUserBonusAsync(string userId, CancellationToken ct = default)
  {
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
    if (user == null) return Result.Fail("User not found.");
    return Result.Ok(user.Coins);
  }

  public async Task<Result> AccrueBonusesAsync(string userId, decimal amount)
  {
    var user = _context.Users.FirstOrDefault(u => u.Id == userId);
    if (user == null) return Result.Fail("User not found.");

    user.Coins += amount;
    await _context.SaveChangesAsync();

    return Result.Ok();
  }
}