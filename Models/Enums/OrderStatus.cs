namespace BoardGamesStore.Models.Enums;

public enum OrderStatusId : int
{
  New = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Completed = 5,
  Cancelled = 6,
  Returned = 7
}