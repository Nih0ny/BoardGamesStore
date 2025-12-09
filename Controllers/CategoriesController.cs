using BoardGamesStore.Models;
using BoardGamesStore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
  private readonly ICategoryService _categoryService = categoryService;

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var categories = await _categoryService.GetAllAsync();
    return Ok(categories);
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    var category = await _categoryService.GetByIdAsync(id);
    return category is null ? NotFound() : Ok(category);
  }

  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
  {
    var result = await _categoryService.CreateAsync(dto);
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
        : BadRequest(result.Errors);
  }

  [HttpPut("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
  {
    var result = await _categoryService.UpdateAsync(id, dto);
    return result.IsSuccess ? Ok(dto) : NotFound();
  }

  [HttpDelete("{id:int}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _categoryService.DeleteAsync(id);
    return result.IsSuccess ? NoContent() : NotFound();
  }
}