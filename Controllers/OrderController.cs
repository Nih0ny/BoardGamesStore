using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _orders.GetAllAsync(q => q
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderItems));
            return Ok(list);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orders.GetByIdAsync(id, q => q
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderItems));
            return order is null ? NotFound() : Ok(order);
        }

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] Order dto)
        {
            var created = await _orders.CreateAsync(dto.UserId, dto.StatusId, dto.Total);
            return Ok(created);
        }

        [Authorize]
        [HttpPut]
        [Route("{id:int}/update")]
        public async Task<IActionResult> Update(int id, [FromBody] Order dto)
        {
            if (id != dto.Id) return BadRequest();
            var ok = await _orders.UpdateAsync(dto);
            return ok ? Ok(dto) : NotFound();
        }

        [Authorize]
        [HttpDelete]
        [Route("{id:int}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _orders.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpPatch]
        [Route("{id:int}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto body)
        {
            var ok = await _orders.ChangeStatusAsync(id, body.StatusId);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost]
        [Route("{id:int}/recalculate")]
        public async Task<IActionResult> Recalculate(int id)
        {
            var total = await _orders.RecalculateTotalAsync(id, true);
            return Ok(new { OrderId = id, Total = total });
        }

        [HttpGet]
        [Route("{orderId:int}/items")]
        public async Task<IActionResult> ListItems(int orderId)
        {
            var items = await _orderItems.GetByOrderAsync(orderId);
            return Ok(items);
        }

        [Authorize]
        [HttpPost]
        [Route("{orderId:int}/items/add")]
        public async Task<IActionResult> AddItem(int orderId, [FromBody] AddItemDto body)
        {
            var item = await _orderItems.AddOrUpdateAsync(orderId, body.ProductId, body.Quantity, body.Price);
            return Ok(item);
        }

        [Authorize]
        [HttpPut]
        [Route("{orderId:int}/items/{orderItemId:int}/update")]
        public async Task<IActionResult> UpdateItem(int orderId, int orderItemId, [FromBody] UpdateQtyDto body)
        {
            var ok = await _orderItems.UpdateQuantityAsync(orderItemId, body.Quantity);
            return ok ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpDelete]
        [Route("{orderId:int}/items/{orderItemId:int}/delete")]
        public async Task<IActionResult> RemoveItem(int orderId, int orderItemId)
        {
            var ok = await _orderItems.RemoveAsync(orderItemId);
            return ok ? NoContent() : NotFound();
        }

        [Authorize]
        [HttpPost]
        [Route("{orderId:int}/items/clear")]
        public async Task<IActionResult> ClearItems(int orderId)
        {
            await _orderItems.ClearAsync(orderId);
            return NoContent();
        }

        // --- Inline DTO definitions ---
        public record ChangeStatusDto(int StatusId);
        public record AddItemDto(int ProductId, int Quantity, decimal Price);
        public record UpdateQtyDto(int Quantity);
    }
}
