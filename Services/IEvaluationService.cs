using FluentResults;

namespace BoardGamesStore.Services;

public interface IEvaluationService
{
  Task<Result> UpsertEvaluationAsync(int productId, string userId, int score, CancellationToken ct = default);
  Task<Result> RemoveEvaluationAsync(int productId, string userId, CancellationToken ct = default);
}