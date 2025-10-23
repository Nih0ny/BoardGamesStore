using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItems;
        private readonly ApplicationDbContext _db;

        public OrderItemController(IOrderItemService orderItems, ApplicationDbContext db)
        {
            _orderItems = orderItems;
            _db = db;
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> Index()
        {
            var list = await _db.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .AsNoTracking()
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : Ok(item);
        }

        // [Authorize(Roles = "Admin")]
        // public IActionResult Create()
        // {
        //     ViewData["OrderId"] = new SelectList(_db.Orders, "Id", "Id");
        //     ViewData["ProductId"] = new SelectList(_db.Products, "Id", "Id");
        //     return View();
        // }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] OrderItem vm)
        {
            await _orderItems.AddOrUpdateAsync(vm.OrderId, vm.ProductId, vm.Quantity, vm.Price);
            return Ok(vm);
        }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpPut]
        [Route("{id:int}/update")]
        public async Task<IActionResult> Edit(int id, [FromBody] OrderItem vm)
        {
            if (id != vm.Id) return BadRequest();
            var ok = await _orderItems.UpdateQuantityAsync(vm.Id, vm.Quantity);
            return ok ? Ok(vm) : NotFound();
        }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpDelete]
        [Route("{id:int}/delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderItems.RemoveAsync(id);
            return NoContent();
        }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpPost]
        [Route("{orderId:int}/clear")]
        public async Task<IActionResult> Clear(int orderId)
        {
            await _orderItems.ClearAsync(orderId);
            return NoContent();
        }
    }
}
