namespace BoardGamesStore.Services.Settings;

public class JwtSettings
{
  public required string Host { get; set; }
  public required string AccessSecret { get; set; }
  public required string AccessAudience { get; set; }
  public required string RefreshSecret { get; set; }
  public required string RefreshAudience { get; set; }
  public required string Issuer { get; set; }
  public int AccessTokenExpirationMinutes { get; set; }
  public int RefreshTokenExpirationDays { get; set; }
}