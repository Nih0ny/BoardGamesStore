using BoardGamesStore.Models;

namespace BoardGamesStore.Interfaces;

public interface ILookupsService
{
  Task<List<OrderStatusDto>> GetOrderStatusesAsync(CancellationToken ct = default);
  Task<List<PaymentStatusDto>> GetPaymentStatusesAsync(CancellationToken ct = default);
  Task<List<ReportStatusDto>> GetReportStatusesAsync(CancellationToken ct = default);
  Task<List<DeliveryMethodDto>> GetDeliveryMethodsAsync(CancellationToken ct = default);
}
