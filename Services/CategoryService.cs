using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class CategoryService(ApplicationDbContext context) : ICategoryService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<IEnumerable<CategoryDto>> GetAllAsync()
  {
    return await _context.Categories
      .AsNoTracking()
      .Select(c => new CategoryDto
      {
        Id = c.Id,
        Name = c.Name
      })
      .ToListAsync();
  }

  public async Task<CategoryDto?> GetByIdAsync(int id)
  {
    return await _context.Categories
      .AsNoTracking()
      .Where(c => c.Id == id)
      .Select(c => new CategoryDto
      {
        Id = c.Id,
        Name = c.Name
      })
      .FirstOrDefaultAsync();
  }

  public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto category)
  {
    if (string.IsNullOrWhiteSpace(category.Name))
      return Result.Fail<CategoryDto>("Category name cannot be empty.");

    if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
      return Result.Fail<CategoryDto>("Category with this name already exists.");

    var newCategory = new Category { Name = category.Name };
    _context.Categories.Add(newCategory);
    await _context.SaveChangesAsync();

    return Result.Ok(new CategoryDto
    {
      Id = newCategory.Id,
      Name = newCategory.Name
    });
  }

  public async Task<Result> UpdateAsync(int id, UpdateCategoryDto category)
  {
    if (string.IsNullOrWhiteSpace(category.Name))
      return Result.Fail("Category name cannot be empty.");

    var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    if (existing is null)
      return Result.Fail("Category not found.");

    if (await _context.Categories.AnyAsync(c => c.Name == category.Name && c.Id != id))
      return Result.Fail("Category with this name already exists.");

    existing.Name = category.Name;
    await _context.SaveChangesAsync();
    return Result.Ok();
  }

  public async Task<Result> DeleteAsync(int id)
  {
    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    if (category is null)
      return Result.Fail("Category not found.");

    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();
    return Result.Ok();
  }
}