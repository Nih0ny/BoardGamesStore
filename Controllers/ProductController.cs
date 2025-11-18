using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _products;
        public ProductController(IProductService products) => _products = products;

        // ---- Basic CRUD (kept) ----

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var ct = HttpContext.RequestAborted;
            var list = await _products.GetAllAsync(ct: ct);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ct = HttpContext.RequestAborted;
            var p = await _products.GetByIdAsync(id, ct: ct);
            return p is null ? NotFound() : Ok(p);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Product dto)
        {
            var ct = HttpContext.RequestAborted;
            var created = await _products.CreateAsync(dto, ct);
            return Ok(created);
        }

        //[Authorize]
        [HttpPut("{id:int}/update")]
        public async Task<IActionResult> Update(int id, [FromBody] Product dto)
        {
            if (id != dto.Id) return BadRequest("Mismatched id.");
            var ct = HttpContext.RequestAborted;
            var ok = await _products.UpdateAsync(dto, ct);
            return ok ? Ok(dto) : NotFound();
        }

        //[Authorize]
        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var ct = HttpContext.RequestAborted;
            var ok = await _products.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        // ---- New: array-returning, product-ready endpoints ----

        // DTO for query binding
        public record SearchRequest(
            string? Text,
            string? Category,
            decimal? MinPrice,
            decimal? MaxPrice,
            bool? InStockOnly,
            string? Sort,
            int? Skip,
            int? Take
        );

        /// <summary>
        /// Flexible search with filters, sorting and pagination.
        /// Examples:
        ///   GET /api/product/search?text=catan&category=Family&minPrice=10&maxPrice=50&inStockOnly=true&sort=price_asc&skip=0&take=20
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchRequest req)
        {
            var ct = HttpContext.RequestAborted;

            var q = new ProductQuery(
                Text: req.Text,
                Category: req.Category,
                MinPrice: req.MinPrice,
                MaxPrice: req.MaxPrice,
                InStockOnly: req.InStockOnly ?? false,
                Sort: req.Sort,
                Skip: req.Skip ?? 0,
                Take: req.Take ?? 20
            );

            var result = await _products.SearchAsync(q, ct);

            // Optional: include total count in a header for client-side pagination components
            Response.Headers["X-Total-Count"] = result.Total.ToString();

            return Ok(result.Items);
        }

        /// <summary>
        /// Batch fetch by IDs: /api/product/by-ids?ids=1&ids=2&ids=3
        /// </summary>
        [HttpGet("by-ids")]
        public async Task<IActionResult> GetByIds([FromQuery] int[] ids)
        {
            var ct = HttpContext.RequestAborted;
            if (ids is null || ids.Length == 0) return Ok(Array.Empty<Product>());
            var items = await _products.GetByIdsAsync(ids, ct);
            return Ok(items);
        }

        /// <summary>
        /// Simple similarity (category + heuristics).
        /// </summary>
        [HttpGet("{id:int}/similar")]
        public async Task<IActionResult> GetSimilar(int id, [FromQuery] int limit = 8)
        {
            var ct = HttpContext.RequestAborted;
            var items = await _products.GetSimilarAsync(id, limit, ct);
            return Ok(items);
        }

        /// <summary>
        /// Recently added products. Optionally filter by category.
        /// </summary>
        [HttpGet("newest")]
        public async Task<IActionResult> GetNewest([FromQuery] int limit = 12, [FromQuery] string? category = null)
        {
            var ct = HttpContext.RequestAborted;
            var items = await _products.GetNewestAsync(limit, category, ct);
            return Ok(items);
        }

        /// <summary>
        /// Categories with product counts (for filters/facets).
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var ct = HttpContext.RequestAborted;
            var list = await _products.GetCategoriesWithCountsAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Price stats for slider UI (min/max/count), optionally within a category.
        /// </summary>
        [HttpGet("stats/price")]
        public async Task<IActionResult> GetPriceStats([FromQuery] string? category = null)
        {
            var ct = HttpContext.RequestAborted;
            var (min, max, count) = await _products.GetPriceStatsAsync(category, ct);
            return Ok(new { min, max, count });
        }

        // ---- (Optional but handy) stock & image helpers: mutations kept protected ----

        public record AdjustStockRequest(int ProductId, int Delta);

        [Authorize]
        [HttpPost("stock/adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var ok = await _products.AdjustStockAsync(req.ProductId, req.Delta, ct);
            return ok ? NoContent() : NotFound();
        }

        public record SetImageRequest(string? ImageUrl);

        [Authorize]
        [HttpPut("{id:int}/image")]
        public async Task<IActionResult> SetImage(int id, [FromBody] SetImageRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var ok = await _products.SetImageUrlAsync(id, req.ImageUrl, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
