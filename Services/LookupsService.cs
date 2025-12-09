using BoardGamesStore.Data;
using BoardGamesStore.Interfaces;
using BoardGamesStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesStore.Services;

public class LookupsService(ApplicationDbContext context) : ILookupsService
{
  private readonly ApplicationDbContext _context = context;

  public async Task<List<OrderStatusDto>> GetOrderStatusesAsync(CancellationToken ct = default)
  {
    return await _context.OrderStatuses
      .OrderBy(os => os.Id)
      .Select(os => new OrderStatusDto
      {
        Id = (int)os.Id,
        Name = os.Name
      })
      .ToListAsync(ct);
  }

  public async Task<List<PaymentStatusDto>> GetPaymentStatusesAsync(CancellationToken ct = default)
  {
    return await _context.PaymentStatuses
      .OrderBy(ps => ps.Id)
      .Select(ps => new PaymentStatusDto
      {
        Id = (int)ps.Id,
        Name = ps.Name
      })
      .ToListAsync(ct);
  }

  public async Task<List<ReportStatusDto>> GetReportStatusesAsync(CancellationToken ct = default)
  {
    return await _context.ReportStatuses
      .OrderBy(rs => rs.Id)
      .Select(rs => new ReportStatusDto
      {
        Id = (int)rs.Id,
        Name = rs.Name
      })
      .ToListAsync(ct);
  }

  public async Task<List<DeliveryMethodDto>> GetDeliveryMethodsAsync(CancellationToken ct = default)
  {
    return await _context.DeliveryMethods
      .Where(dm => dm.IsActive)
      .OrderBy(dm => dm.Id)
      .Select(dm => new DeliveryMethodDto
      {
        Id = (int)dm.Id,
        Name = dm.Name,
        Description = dm.Description,
        BasePrice = dm.BasePrice,
        IsActive = dm.IsActive
      })
      .ToListAsync(ct);
  }
}
