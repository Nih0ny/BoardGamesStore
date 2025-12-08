using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Services;

public interface IUserService
{
  Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken ct = default);
  Task<Result> AccrueBonusesAsync(string userId, decimal amount, string reason, CancellationToken ct = default);
}