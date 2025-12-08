using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Services;

public class UserService : IUserService
{
  public Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task<Result> AccrueBonusesAsync(string userId, decimal amount, string reason, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }
}