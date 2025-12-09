using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IBonusService
{
  Task<Result<decimal>> GetUserBonusAsync(string userId, CancellationToken ct = default);
  Task<Result> AccrueBonusesAsync(string userId, decimal amount);
}