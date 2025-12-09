using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface ICategoryService
{
  Task<IEnumerable<CategoryDto>> GetAllAsync();
  Task<CategoryDto?> GetByIdAsync(int id);
  Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto category);
  Task<Result> UpdateAsync(int id, UpdateCategoryDto category);
  Task<Result> DeleteAsync(int id);
}