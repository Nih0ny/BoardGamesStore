using BoardGamesStore.Models;
using FluentResults;

namespace BoardGamesStore.Interfaces;

public interface IPaymentService
{
  Task<PaymentResponseDto?> CreatePaymentAsync(int orderId, string userId, string serverUrl, string clientUrl);
  Task<Result> HandlePaymentCallbackAsync(string data, string signature);
}