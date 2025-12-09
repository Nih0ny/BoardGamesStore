using Microsoft.AspNetCore.Mvc;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController(ILookupsService lookupsService) : ControllerBase
{
  private readonly ILookupsService _lookupsService = lookupsService;

  [HttpGet("order-statuses")]
  public async Task<IActionResult> GetOrderStatuses(CancellationToken ct = default)
  {
    var statuses = await _lookupsService.GetOrderStatusesAsync(ct);
    return Ok(statuses);
  }

  [HttpGet("payment-statuses")]
  public async Task<IActionResult> GetPaymentStatuses(CancellationToken ct = default)
  {
    var statuses = await _lookupsService.GetPaymentStatusesAsync(ct);
    return Ok(statuses);
  }

  [HttpGet("report-statuses")]
  public async Task<IActionResult> GetReportStatuses(CancellationToken ct = default)
  {
    var statuses = await _lookupsService.GetReportStatusesAsync(ct);
    return Ok(statuses);
  }

  [HttpGet("delivery-methods")]
  public async Task<IActionResult> GetDeliveryMethods(CancellationToken ct = default)
  {
    var methods = await _lookupsService.GetDeliveryMethodsAsync(ct);
    return Ok(methods);
  }
}

