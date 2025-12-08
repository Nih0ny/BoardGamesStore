using BoardGamesStore.Data;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class EvaluationService(ApplicationDbContext context) : IEvaluationService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<Result> UpsertEvaluationAsync(int productId, string userId, int score, CancellationToken ct)
  {
    if (score < 1 || score > 5)
      return Result.Fail("Score must be between 1 and 5.");

    var evaluation = await _context.Evaluations
        .FirstOrDefaultAsync(e => e.ProductId == productId && e.UserId == userId, ct);

    if (evaluation != null)
    {
      evaluation.Rating = score;
    }
    else
    {
      evaluation = new Evaluation
      {
        ProductId = productId,
        UserId = userId,
        Rating = score,
        CreatedAt = DateTime.UtcNow
      };
      _context.Evaluations.Add(evaluation);
    }

    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> RemoveEvaluationAsync(int productId, string userId, CancellationToken ct)
  {
    var evaluation = await _context.Evaluations
        .FirstOrDefaultAsync(e => e.ProductId == productId && e.UserId == userId, ct);

    if (evaluation == null)
    {
      return Result.Ok();
    }

    _context.Evaluations.Remove(evaluation);
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }
}