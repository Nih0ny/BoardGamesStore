using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;                // for Include in service include expressions
using BoardGamesStore.Models;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orders;
        private readonly IOrderItemService _orderItems;

        public OrderController(IOrderService orders, IOrderItemService orderItems)
        {
            _orders = orders;
            _orderItems = orderItems;
        }

        // GET: api/order
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _orders.GetAllAsync(q => q
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderItems));
            return Ok(list);
        }

        // GET: api/order/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orders.GetByIdAsync(id, q => q
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderItems));
            return order is null ? NotFound() : Ok(order);
        }

        // POST: api/order   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Order dto)
        {
            // expects dto.UserId (string), dto.StatusId (int), dto.Total (decimal)
            var created = await _orders.CreateAsync(dto.UserId, dto.StatusId, dto.Total);
            return Ok(created);
        }

        // PUT: api/order/5   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Order dto)
        {
            if (id != dto.Id) return BadRequest("Mismatched id.");
            var ok = await _orders.UpdateAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        // DELETE: api/order/5   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _orders.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // PATCH: api/order/5/status   (Admin only)  body: { "statusId": 2 }
        public record ChangeStatusDto(int StatusId);

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto body)
        {
            var ok = await _orders.ChangeStatusAsync(id, body.StatusId);
            return ok ? NoContent() : NotFound();
        }

        // POST: api/order/5/recalculate (recomputes Total from items)
        [HttpPost("{id:int}/recalculate")]
        public async Task<IActionResult> Recalculate(int id)
        {
            var total = await _orders.RecalculateTotalAsync(id, save: true);
            return Ok(new { OrderId = id, Total = total });
        }

        // ===== Items on an order =====

        // GET: api/order/5/items
        [HttpGet("{orderId:int}/items")]
        public async Task<IActionResult> ListItems(int orderId)
        {
            var items = await _orderItems.GetByOrderAsync(orderId);
            return Ok(items);
        }

        // POST: api/order/5/items   (Admin only)
        // body: { "productId": 1, "quantity": 2, "price": 99.00 }
        public record AddItemDto(int ProductId, int Quantity, decimal Price);

        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId:int}/items")]
        public async Task<IActionResult> AddItem(int orderId, [FromBody] AddItemDto body)
        {
            var item = await _orderItems.AddOrUpdateAsync(orderId, body.ProductId, body.Quantity, body.Price);
            return Ok(item);
        }

        // PUT: api/order/5/items/12   (Admin only)  body: { "quantity": 3 }
        public record UpdateQtyDto(int Quantity);

        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId:int}/items/{orderItemId:int}")]
        public async Task<IActionResult> UpdateItem(int orderId, int orderItemId, [FromBody] UpdateQtyDto body)
        {
            var ok = await _orderItems.UpdateQuantityAsync(orderItemId, body.Quantity);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: api/order/5/items/12   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{orderId:int}/items/{orderItemId:int}")]
        public async Task<IActionResult> RemoveItem(int orderId, int orderItemId)
        {
            var ok = await _orderItems.RemoveAsync(orderItemId);
            return ok ? NoContent() : NotFound();
        }

        // POST: api/order/5/items/clear   (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId:int}/items/clear")]
        public async Task<IActionResult> ClearItems(int orderId)
        {
            await _orderItems.ClearAsync(orderId);
            return NoContent();
        }
    }
}
