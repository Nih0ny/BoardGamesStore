namespace BoardGamesStore.Services.Settings;

public class SmtpSettings
{
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; } = 587;
  public string User { get; set; } = string.Empty;
  public string From { get; set; } = string.Empty;
  public bool StartTls { get; set; } = true;
  public string RefreshToken { get; set; } = string.Empty;
  public string ClientId { get; set; } = string.Empty;
  public string ClientSecret { get; set; } = string.Empty;
}