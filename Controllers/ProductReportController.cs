using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // if you pass include expr
using BoardGamesStore.Services;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;

// namespace BoardGamesStore.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ProductReportController(IProductReportService reports) : ControllerBase
//     {
//         private readonly IProductReportService _reports = reports;

//         // // MVC View:
//         // public async Task<IActionResult> Index() => View(...);

//         [HttpGet]
//         [Route("all")]
//         public async Task<IActionResult> GetAll()
//         {
//             var list = await _reports.GetAllAsync(q => q.Include(r => r.Product).Include(r => r.User));
//             return Ok(list);
//         }

//         // // MVC View:
//         // public async Task<IActionResult> Details(int? id) => View(productReport);

//         [HttpGet]
//         [Route("{id:int}")]
//         public async Task<IActionResult> GetById(int id)
//         {
//             var r = await _reports.GetByIdAsync(id, q => q.Include(x => x.Product).Include(x => x.User));
//             return r is null ? NotFound() : Ok(r);
//         }

//         // // MVC View (GET):
//         // public IActionResult Create() => View();

//         [HttpPost]
//         [Route("create")]
//         public async Task<IActionResult> Create([FromBody] CreateDto body)
//         {
//             // body: { "userId":"...", "productId":1, "reason":"...", "status":"new" }
//             var created = await _reports.CreateAsync(body.UserId, body.ProductId, body.Reason ?? "", body.Status ?? "new");
//             return Ok(created);
//         }
//         public record CreateDto(string UserId, int ProductId, string? Reason, string? Status);

//         // // MVC View (GET):
//         // public async Task<IActionResult> Edit(int? id) => View(report);

//         [Authorize]
//         [HttpPut]
//         [Route("{id:int}/update")]
//         public async Task<IActionResult> Update(int id, [FromBody] ProductReport dto)
//         {
//             if (id != dto.Id) return BadRequest("Mismatched id.");
//             var ok = await _reports.UpdateAsync(dto);
//             return ok ? Ok(dto) : NotFound();
//         }

//         // // MVC View (GET+POST):
//         // public async Task<IActionResult> Delete(int? id) => View(report);
//         // [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) ...

//         [Authorize]
//         [HttpDelete]
//         [Route("{id:int}/delete")]
//         public async Task<IActionResult> Delete(int id)
//         {
//             var ok = await _reports.DeleteAsync(id);
//             return ok ? NoContent() : NotFound();
//         }

//         [Authorize]
//         [HttpPatch]
//         [Route("{id:int}/status")]
//         public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto body)
//         {
//             var ok = await _reports.ChangeStatusAsync(id, body.Status);
//             return ok ? NoContent() : NotFound();
//         }
//         public record ChangeStatusDto(string Status);
//     }
// }
