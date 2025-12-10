using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Interfaces;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService productService, IProductImageService imageService) : ControllerBase
{
	private readonly IProductService _productService = productService;
	private readonly IProductImageService _imageService = imageService;

	[HttpGet]
	public async Task<IActionResult> Search([FromQuery] ProductSearchQuery query, CancellationToken ct)
	{
		var result = await _productService.SearchAsync(query, ct);
		Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
		return Ok(result.Items);
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id, CancellationToken ct)
	{
		var product = await _productService.GetByIdAsync(id, ct);
		return product is null ? NotFound() : Ok(product);
	}

	[HttpGet("{id:int}/similar")]
	public async Task<IActionResult> GetSimilar(int id, [FromQuery] int limit = 5, CancellationToken ct = default)
	{
		var similar = await _productService.GetSimilarAsync(id, limit, ct);
		return Ok(similar);
	}

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
	{
		var created = await _productService.CreateAsync(dto, ct);
		return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
	}

	[HttpPatch("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
	{
		var result = await _productService.UpdateAsync(id, dto, ct);
		return result.IsSuccess ? Ok() : NotFound();
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken ct)
	{
		var result = await _productService.DeleteAsync(id, ct);
		return result.IsSuccess ? NoContent() : NotFound();
	}

	[HttpPatch("{id:int}/discount")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> SetDiscount(int id, [FromBody] SetProductDiscountDto dto, CancellationToken ct)
	{
		var result = await _productService.SetDiscountAsync(id, dto, ct);
		return result.IsSuccess ? NoContent() : BadRequest(result.Errors.Select(e => e.Message));
	}

	[HttpPost("{productId:int}/images")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> UploadImage(int productId, IFormFile file, [FromQuery] bool isMainImage = false, CancellationToken ct = default)
	{
		var result = await _imageService.UploadImageAsync(productId, file, isMainImage, ct);
		if (result.IsFailed)
			return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

		return CreatedAtAction(nameof(GetProductImages), new { productId }, result.Value);
	}

	[HttpGet("{productId:int}/images")]
	public async Task<IActionResult> GetProductImages(int productId, CancellationToken ct = default)
	{
		var result = await _imageService.GetProductImagesAsync(productId, ct);
		if (result.IsFailed)
			return NotFound(new { errors = result.Errors.Select(e => e.Message) });

		return Ok(result.Value);
	}

	[HttpGet("{productId:int}/images/main")]
	public async Task<IActionResult> GetMainImage(int productId, CancellationToken ct = default)
	{
		var result = await _imageService.GetMainImageAsync(productId, ct);
		if (result.IsFailed)
			return NotFound(new { errors = result.Errors.Select(e => e.Message) });

		return Ok(result.Value);
	}

	[HttpPut("{productId:int}/images/{imageId:int}/main")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> SetMainImage(int productId, int imageId, CancellationToken ct = default)
	{
		var result = await _imageService.SetMainImageAsync(productId, imageId, ct);
		if (result.IsFailed)
			return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

		return NoContent();
	}

	[HttpDelete("images/{imageId:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> DeleteImage(int imageId, CancellationToken ct = default)
	{
		var result = await _imageService.DeleteImageAsync(imageId, ct);
		if (result.IsFailed)
			return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

		return NoContent();
	}

	[HttpPut("{productId:int}/images/reorder")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> ReorderImages(int productId, [FromBody] List<(int imageId, int displayOrder)> ordering, CancellationToken ct = default)
	{
		var result = await _imageService.ReorderImagesAsync(productId, ordering, ct);
		if (result.IsFailed)
			return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

		return NoContent();
	}
}
