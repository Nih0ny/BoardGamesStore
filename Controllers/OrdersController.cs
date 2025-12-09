using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Models;
using BoardGamesStore.Models.Entities;
using BoardGamesStore.Interfaces;
using System.Security.Claims;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
  private readonly IOrderService _orderService = orderService;

  private string? GetCurrentUserId()
  {
    return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  }

  [Authorize(Roles = "Admin")]
  [HttpGet]
  public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
  {
    var result = await _orderService.GetAllAsync(pageNumber, pageSize);
    return Ok(result);
  }

  [Authorize]
  [HttpGet("my")]
  public async Task<IActionResult> GetUserOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
  {
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var result = await _orderService.GetByUserIdAsync(userId, pageNumber, pageSize);
    return Ok(result);
  }

  [Authorize(Roles = "Admin")]
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    var order = await _orderService.GetByIdAsync(id);
    return order is null ? NotFound() : Ok(order);
  }

  [Authorize]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
  {
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var address = new Address
    {
      City = dto.DeliveryAddress.City,
      Street = dto.DeliveryAddress.Street,
      Building = dto.DeliveryAddress.Building,
      Apartment = dto.DeliveryAddress.Apartment,
      PostalCode = dto.DeliveryAddress.PostalCode,
      Region = dto.DeliveryAddress.Region
    };

    var result = await _orderService.CreateAsync(
      userId,
      dto.Items,
      dto.RecipientName,
      dto.RecipientPhone,
      address,
      dto.DeliveryMethod
    );

    if (result.IsFailed) return BadRequest(result.Errors);

    return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
  }
  [Authorize]
  [HttpDelete("{id:int}")]
  public async Task<IActionResult> Delete(int id)
  {
    var userId = GetCurrentUserId();
    var order = await _orderService.GetByIdAsync(id);

    if (order is null) return NotFound();
    if (order.UserId != userId && !User.IsInRole("Admin")) return Forbid();

    var result = await _orderService.DeleteAsync(id);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  [Authorize(Roles = "Admin")]
  [HttpPatch("{id:int}/status")]
  public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeOrderStatusDto dto)
  {
    var result = await _orderService.ChangeStatusAsync(id, dto.StatusId);
    return result.IsSuccess ? NoContent() : NotFound();
  }

  [Authorize(Roles = "Admin")]
  [HttpPatch("{id:int}/payment-status")]
  public async Task<IActionResult> ChangePaymentStatus(int id, [FromBody] ChangeOrderPaymentStatusDto dto)
  {
    var result = await _orderService.ChangePaymentStatusAsync(id, dto.StatusId);
    return result.IsSuccess ? NoContent() : NotFound();
  }
}