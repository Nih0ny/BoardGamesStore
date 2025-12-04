using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Services;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orders) : ControllerBase
{
  private readonly IOrderService _orders = orders;

  [Authorize]
  [HttpGet]
  public async Task<IActionResult> GetAll(int userPage = 1, int itemPage = 1)
  {
    var result = await _orders.GetAllAsync(userPage, itemPage);
    return Ok(result);
  }

  [Authorize(Roles = "Admin")]
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    var order = await _orders.GetByIdAsync(id);
    return order is null ? NotFound() : Ok(order);
  }

  [Authorize]
  [HttpGet]
  [Route("my")]
  public async Task<IActionResult> GetUserOrders(int userPage = 1, int itemPage = 1)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var orders = await _orders.GetByUserIdAsync(userId, userPage, itemPage);
    return Ok(orders);
  }

  [HttpPost]
  [Authorize] //TODO: Admin
  public async Task<IActionResult> Create([FromBody] Order dto)
  {
    var result = await _orders.CreateAsync(dto.UserId, dto.StatusId);
    if (result.IsFailed) return BadRequest(result.Errors);
    return Ok(result.Value);
  }

  // [Authorize("Admin")] //TODO: Admin
  // [HttpPut("{id:int}")]
  // public async Task<IActionResult> Update(int id, [FromBody] Order dto)
  // {
  //   if (id != dto.Id) return BadRequest();
  //   var ok = await _orders.UpdateAsync(dto);
  //   return ok ? Ok(dto) : NotFound();
  // }

  [Authorize(Roles = "Admin")] //TODO: Admin
  [HttpDelete("{id:int}")]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _orders.DeleteAsync(id);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  [Authorize(Roles = "Admin")] //TODO: Admin
  [HttpPatch]
  [Route("{id:int}/status")]
  public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto body)
  {
    var result = await _orders.ChangeStatusAsync(id, body.StatusId);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  // [HttpPost]
  // [Route("{id:int}/recalculate")]
  // public async Task<IActionResult> Recalculate(int id)
  // {
  //   var total = await _orders.RecalculateTotalAsync(id, true);
  //   return Ok(new { OrderId = id, Total = total });
  // }

  // [HttpGet]
  // [Route("{orderId:int}/items")]
  // public async Task<IActionResult> ListItems(int orderId)
  // {
  //   var items = await _orders.GetByOrderAsync(orderId);
  //   return Ok(items);
  // }

  // [Authorize] //TODO: Admin
  // [HttpPost]
  // [Route("{orderId:int}/items/add")]
  // public async Task<IActionResult> AddItem(int orderId, [FromBody] AddItemDto body)
  // {
  //   var item = await _orderItems.AddOrUpdateAsync(orderId, body.ProductId, body.Quantity, body.Price);
  //   return Ok(item);
  // }

  // [Authorize] //TODO: Admin
  // [HttpPut]
  // [Route("{orderId:int}/items/{orderItemId:int}/update")]
  // public async Task<IActionResult> UpdateItem(int orderId, int orderItemId, [FromBody] UpdateQtyDto body)
  // {
  //   var ok = await _orderItems.UpdateQuantityAsync(orderItemId, body.Quantity);
  //   return ok ? NoContent() : NotFound();
  // }

  // [Authorize] //TODO: Admin
  // [HttpDelete]
  // [Route("{orderId:int}/items/{orderItemId:int}/delete")]
  // public async Task<IActionResult> RemoveItem(int orderId, int orderItemId)
  // {
  //     var ok = await _orderItems.RemoveAsync(orderItemId);
  //     return ok ? NoContent() : NotFound();
  // }

  // [Authorize] //TODO: Admin
  // [HttpPost]
  // [Route("{orderId:int}/items/clear")]
  // public async Task<IActionResult> ClearItems(int orderId)
  // {
  //     await _orderItems.ClearAsync(orderId);
  //     return NoContent();
  // }

  // --- Inline DTO definitions ---
  public record ChangeStatusDto(int StatusId);
  public record AddItemDto(int ProductId, int Quantity, decimal Price);
  public record UpdateQtyDto(int Quantity);
}
