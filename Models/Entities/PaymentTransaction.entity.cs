
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BoardGamesStore.Models.Entities;

public class PaymentTransaction
{
  [Key]
  public int Id { get; set; }

  public int OrderId { get; set; }
  public required Order Order { get; set; }

  public string? PaymentSystem { get; set; }
  public string? TransactionId { get; set; }
  public decimal Amount { get; set; }
  public string? Status { get; set; }

  [System.ComponentModel.DataAnnotations.Schema.NotMapped]
  public LiqPayStatus StatusEnum
  {
    get
    {
      if (string.IsNullOrEmpty(Status)) return LiqPayStatus.Unknown;

      return Status switch
      {
        "success" => LiqPayStatus.Success,
        "sandbox" => LiqPayStatus.Sandbox,
        "failure" => LiqPayStatus.Failure,
        "error" => LiqPayStatus.Error,
        "wait_secure" => LiqPayStatus.WaitSecure,
        "wait_accept" => LiqPayStatus.WaitAccept,
        "processing" => LiqPayStatus.Processing,
        "reversed" => LiqPayStatus.Reversed,
        _ => LiqPayStatus.Unknown
      };
    }
  }

  public DateTime CreatedAt { get; set; }
}

public enum LiqPayStatus
{
  Unknown = 0,

  [EnumMember(Value = "success")]
  Success,

  [EnumMember(Value = "sandbox")]
  Sandbox,

  [EnumMember(Value = "subscribed")]
  Subscribed,

  [EnumMember(Value = "failure")]
  Failure,

  [EnumMember(Value = "error")]
  Error,

  [EnumMember(Value = "reversed")]
  Reversed,

  [EnumMember(Value = "wait_secure")]
  WaitSecure,

  [EnumMember(Value = "wait_accept")]
  WaitAccept,

  [EnumMember(Value = "processing")]
  Processing
}
