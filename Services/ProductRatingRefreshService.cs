using BoardGamesStore.Data;
using BoardGamesStore.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class ProductRatingRefreshService(IServiceProvider serviceProvider, ILogger<ProductRatingRefreshService> logger) : BackgroundService
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly ILogger<ProductRatingRefreshService> _logger = logger;
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

      await RefreshStatsAsync(stoppingToken);
    }
  }

  private async Task RefreshStatsAsync(CancellationToken token)
  {
    using var scope = _serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.ExecuteSqlRawAsync(
        "REFRESH MATERIALIZED VIEW CONCURRENTLY product_stats_mv;",
        token);

    _logger.LogInformation("Product ratings materialized view refreshed at: {time}", DateTimeOffset.Now);

    var statsList = await context.Set<ProductRatingSummary>()
      .AsNoTracking()
      .ToListAsync(token);

    _logger.LogInformation("Fetched {Count} items from View.", statsList.Count);

    foreach (var item in statsList)
    {
      _logger.LogInformation("Product ID: {Id}, Avg Rating: {Rating}, Count: {Count}",
          item.ProductId, item.AverageRating, item.ReviewsCount);
    }
  }
}