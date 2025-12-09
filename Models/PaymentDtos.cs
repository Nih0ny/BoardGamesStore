using LiqPay.SDK.Dto;

namespace BoardGamesStore.Models;

public class PaymentRequestDto
{
  public int OrderId { get; init; }
}

public class PaymentResponseDto
{
  public string Data { get; init; } = null!;
  public string Signature { get; init; } = null!;
  public string CheckoutUrl { get; init; } = "https://www.liqpay.ua/api/3/checkout";
}

public class LiqPayResponseWithInfo : LiqPayResponse
{
  public InfoData Info { get; set; } = new InfoData();
}

public class InfoData
{
  public string UserId { get; set; } = string.Empty;
  public decimal Bonus { get; set; }
}