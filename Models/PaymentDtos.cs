using LiqPay.SDK.Dto;

namespace BoardGamesStore.Models;

public class PaymentRequestDto
{
  public int OrderId { get; set; }
}

public class PaymentResponseDto
{
  public string Data { get; set; } = string.Empty;
  public string Signature { get; set; } = string.Empty;
  public string CheckoutUrl { get; set; } = "https://www.liqpay.ua/api/3/checkout";
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
