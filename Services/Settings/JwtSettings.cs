namespace BoardGamesStore.Services.Settings;

public class JwtSettings
{
  public required string Secret { get; set; }
  public required string Audience { get; set; }
  public required string Issuer { get; set; }
  public int TokenExpirationMinutes { get; set; }
}