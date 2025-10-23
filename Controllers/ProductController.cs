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

        // // MVC View:
        // public async Task<IActionResult> Index() => View(await _products.GetAllAsync());

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _products.GetAllAsync();
            return Ok(list);
        }

        // // MVC View:
        // public async Task<IActionResult> Details(int? id) { ... return View(product); }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _products.GetByIdAsync(id);
            return p is null ? NotFound() : Ok(p);
        }

        // // MVC View (GET):
        // public IActionResult Create() => View();

        //
        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] Product dto)
        {
            var created = await _products.CreateAsync(dto);
            return Ok(created);
        }

        // // MVC View (GET):
        // public async Task<IActionResult> Edit(int? id) => View(product);

        //
        [Authorize]
        [HttpPut]
        [Route("{id:int}/update")]
        public async Task<IActionResult> Update(int id, [FromBody] Product dto)
        {
            if (id != dto.Id) return BadRequest("Mismatched id.");
            var ok = await _products.UpdateAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        // // MVC View (GET+POST):
        // public async Task<IActionResult> Delete(int? id) => View(product);
        // [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) ...
        //
        [Authorize]
        [HttpDelete]
        [Route("{id:int}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _products.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
