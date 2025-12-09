using FluentResults;
using BoardGamesStore.Models;

namespace BoardGamesStore.Interfaces;

public interface IProductImageService
{
  Task<Result<ProductImageUploadResult>> UploadImageAsync(int productId, IFormFile file, bool isMainImage = false, CancellationToken ct = default);

  Task<Result<List<ProductImageDto>>> GetProductImagesAsync(int productId, CancellationToken ct = default);

  Task<Result<ProductImageDto>> GetMainImageAsync(int productId, CancellationToken ct = default);

  Task<Result> SetMainImageAsync(int productId, int imageId, CancellationToken ct = default);

  Task<Result> DeleteImageAsync(int imageId, CancellationToken ct = default);

  Task<Result> ReorderImagesAsync(int productId, List<(int imageId, int displayOrder)> ordering, CancellationToken ct = default);
}
