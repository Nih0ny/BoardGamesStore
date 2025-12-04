using BoardGamesStore.Services;
using LiqPay.SDK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api")]
public class PaymentsController(IPaymentService paymentService) : Controller
{
  private readonly IPaymentService _paymentService = paymentService;

  [HttpPost("orders/{orderId:int}/[controller]")]
  [Authorize]
  public async Task<IActionResult> ProcessPayment(int orderId, [FromServices] IHttpContextAccessor httpContextAccessor)
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var paymentResponse = await _paymentService.CreatePaymentAsync(orderId, userId, "https://leda-overdelicious-zuri.ngrok-free.dev/api/payments/callback", "https://leda-overdelicious-zuri.ngrok-free.dev/");
    if (paymentResponse != null)
    {
      return Ok(paymentResponse);
    }
    return BadRequest("Payment processing failed");
  }

  [HttpPost]
  [Route("[controller]/callback")]
  public async Task<IActionResult> PaymentCallback()
  {
    var data = Request.Form["data"].ToString();
    var signature = Request.Form["signature"].ToString();

    if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(signature))
    {
      return BadRequest("Missing data or signature");
    }

    var result = await _paymentService.HandlePaymentCallbackAsync(data, signature);

    if (result.IsFailed)
    {
      return BadRequest();
    }

    return Ok();
  }
}