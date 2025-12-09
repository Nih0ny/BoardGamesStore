using System.Reflection.Metadata;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services.Settings;
using LiqPay.SDK;
using LiqPay.SDK.Dto;
using LiqPay.SDK.Dto.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using FluentResults;
using BoardGamesStore.Interfaces;

namespace BoardGamesStore.Services;

public class PaymentService(ApplicationDbContext context, IOptions<PaymentSettings> paymentSettings, IOrderService orderService, IBonusService bonusService) : IPaymentService
{
  private readonly ApplicationDbContext _context = context;
  private readonly PaymentSettings _paymentSettings = paymentSettings.Value;
  private readonly IOrderService _orderService = orderService;
  private readonly IBonusService _bonusService = bonusService;

  public async Task<PaymentResponseDto?> CreatePaymentAsync(int orderId, string userId, string serverUrl, string clientUrl)
  {
    var order = await _orderService.GetByIdAsync(orderId);
    if (order == null || order.Total <= 0)
    {
      return null;
    }

    LiqPayClient liqpay = new(_paymentSettings.PublicKey, _paymentSettings.PrivateKey);

    var paymentParams = new LiqPayRequest
    {
      Version = 3,
      Action = LiqPayRequestAction.Pay,
      Amount = (double)order.Total,
      Currency = "UAH",
      Description = "Оплата замовлення № " + order.Id,
      OrderId = order.Id.ToString(),
      ServerUrl = serverUrl,
      ResultUrl = clientUrl,
      OtherParams = new Dictionary<string, string>
      {
        { "info", $"{{\"userId\": \"{userId}\", \"bonus\": \"{order.BonusTotal}\"}}" }
      }
    };

    var paramsSdk = liqpay.GenerateDataAndSignature(paymentParams);

    return new PaymentResponseDto
    {
      Data = paramsSdk.Key,
      Signature = paramsSdk.Value
    };
  }

  public async Task<Result> HandlePaymentCallbackAsync(string data, string signature)
  {
    var liqPayClient = new LiqPayClient(_paymentSettings.PublicKey, _paymentSettings.PrivateKey);

    if (liqPayClient.CreateSignature(data) != signature)
    {
      Console.WriteLine("LiqPay Callback: Invalid Signature!");
      return Result.Fail("Invalid Signature");
    }

    var json = Encoding.UTF8.GetString(Convert.FromBase64String(data));
    var response = JsonSerializer.Deserialize<LiqPayResponseWithInfo>(json);

    if (response != null && (response.Status == LiqPayResponseStatus.Success || response.Status == LiqPayResponseStatus.Sandbox))
    {
      await _bonusService.AccrueBonusesAsync(response.Info.UserId, response.Info.Bonus);
      // var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == response.Info.UserId);
      // if (user != null)
      // {
      //   user.Coins += response.Info.Bonus;
      //   await _context.SaveChangesAsync();
      //   Console.WriteLine($"Added {response.Info.Bonus} bonus coins to UserId: {user.Id}");
      // }

      Console.WriteLine($"Payment successful for OrderId: {response.OrderId}");

      return Result.Ok();
    }
    else
    {
      Console.WriteLine($"Payment status for OrderId {response?.OrderId}: {response?.Status}");
      return Result.Fail("Payment failed");
    }
  }
}