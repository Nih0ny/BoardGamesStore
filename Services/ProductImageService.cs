using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class ProductImageService(ApplicationDbContext context, IWebHostEnvironment env) : IProductImageService
{
  private readonly ApplicationDbContext _context = context;
  private readonly IWebHostEnvironment _env = env;
  private const int MaxFileSizeMb = 5;
  private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

  public async Task<Result<ProductImageUploadResult>> UploadImageAsync(int productId, IFormFile file, bool isMainImage = false, CancellationToken ct = default)
  {
    if (file == null || file.Length == 0)
      return Result.Fail<ProductImageUploadResult>("File is empty.");

    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      return Result.Fail<ProductImageUploadResult>($"Product '{productId}' not found.");

    var validationResult = ValidateFile(file);
    if (validationResult.IsFailed)
      return Result.Fail<ProductImageUploadResult>(validationResult.Errors.Select(e => e.Message).ToList());

    try
    {
      var fileName = GenerateFileName(file.FileName);
      var relativePath = $"images/{productId}";
      var fullPath = Path.Combine(_env.WebRootPath, relativePath);

      Directory.CreateDirectory(fullPath);

      var filePath = Path.Combine(fullPath, fileName);
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await file.CopyToAsync(stream, ct);
      }

      var displayOrder = await _context.ProductImages
        .Where(pi => pi.ProductId == productId)
        .MaxAsync(pi => (int?)pi.DisplayOrder, ct) ?? 0;

      if (isMainImage)
      {
        var currentMainImage = await _context.ProductImages
          .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsMainImage, ct);
        if (currentMainImage != null)
        {
          currentMainImage.IsMainImage = false;
        }
      }

      var productImage = new ProductImage
      {
        ProductId = productId,
        Product = product,
        FileName = fileName,
        RelativePath = $"{relativePath}/{fileName}",
        IsMainImage = isMainImage,
        DisplayOrder = displayOrder + 1,
        CreatedAt = DateTime.UtcNow
      };

      _context.ProductImages.Add(productImage);
      await _context.SaveChangesAsync(ct);

      return Result.Ok(new ProductImageUploadResult
      {
        ImageId = productImage.Id,
        RelativePath = productImage.RelativePath,
        IsMainImage = productImage.IsMainImage
      });
    }
    catch (Exception ex)
    {
      return Result.Fail<ProductImageUploadResult>($"Failed to upload file: {ex.Message}");
    }
  }

  public async Task<Result<List<ProductImageDto>>> GetProductImagesAsync(int productId, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      return Result.Fail<List<ProductImageDto>>($"Product '{productId}' not found.");

    var images = await _context.ProductImages
      .AsNoTracking()
      .Where(pi => pi.ProductId == productId)
      .OrderBy(pi => pi.DisplayOrder)
      .Select(pi => new ProductImageDto
      {
        Id = pi.Id,
        RelativePath = pi.RelativePath,
        IsMainImage = pi.IsMainImage,
        DisplayOrder = pi.DisplayOrder
      })
      .ToListAsync(ct);

    return Result.Ok(images);
  }

  public async Task<Result<ProductImageDto>> GetMainImageAsync(int productId, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      return Result.Fail<ProductImageDto>($"Product '{productId}' not found.");

    var mainImage = await _context.ProductImages
      .AsNoTracking()
      .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsMainImage, ct);

    if (mainImage == null)
      return Result.Fail<ProductImageDto>("No main image found for this product.");

    return Result.Ok(new ProductImageDto
    {
      Id = mainImage.Id,
      RelativePath = mainImage.RelativePath,
      IsMainImage = mainImage.IsMainImage,
      DisplayOrder = mainImage.DisplayOrder
    });
  }

  public async Task<Result> SetMainImageAsync(int productId, int imageId, CancellationToken ct = default)
  {
    var image = await _context.ProductImages
      .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == productId, ct);
    if (image == null)
      return Result.Fail("Image not found for this product.");

    var currentMainImage = await _context.ProductImages
      .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsMainImage && pi.Id != imageId, ct);
    if (currentMainImage != null)
    {
      currentMainImage.IsMainImage = false;
    }

    image.IsMainImage = true;
    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  public async Task<Result> DeleteImageAsync(int imageId, CancellationToken ct = default)
  {
    var image = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.Id == imageId, ct);
    if (image == null)
      return Result.Fail("Image not found.");

    try
    {
      var filePath = Path.Combine(_env.WebRootPath, image.RelativePath);
      if (File.Exists(filePath))
      {
        File.Delete(filePath);
      }

      _context.ProductImages.Remove(image);
      await _context.SaveChangesAsync(ct);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Failed to delete image: {ex.Message}");
    }
  }

  public async Task<Result> ReorderImagesAsync(int productId, List<(int imageId, int displayOrder)> ordering, CancellationToken ct = default)
  {
    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
    if (product == null)
      return Result.Fail($"Product '{productId}' not found.");

    var imageIds = ordering.Select(o => o.imageId).ToList();
    var images = await _context.ProductImages
      .Where(pi => pi.ProductId == productId && imageIds.Contains(pi.Id))
      .ToListAsync(ct);

    if (images.Count != ordering.Count)
      return Result.Fail("Some images do not belong to this product.");

    foreach (var (imageId, displayOrder) in ordering)
    {
      var image = images.FirstOrDefault(i => i.Id == imageId);
      if (image != null)
      {
        image.DisplayOrder = displayOrder;
      }
    }

    await _context.SaveChangesAsync(ct);
    return Result.Ok();
  }

  private static Result ValidateFile(IFormFile file)
  {
    var extension = Path.GetExtension(file.FileName).ToLower();
    if (!AllowedExtensions.Contains(extension))
      return Result.Fail($"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

    var fileSizeInMb = file.Length / (1024f * 1024f);
    if (fileSizeInMb > MaxFileSizeMb)
      return Result.Fail($"File size ({fileSizeInMb:F2} MB) exceeds maximum allowed size ({MaxFileSizeMb} MB).");

    return Result.Ok();
  }

  private static string GenerateFileName(string originalFileName)
  {
    var extension = Path.GetExtension(originalFileName);
    return $"{Guid.NewGuid()}{extension}";
  }
}
