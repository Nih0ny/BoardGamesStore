// SmtpClientPooledObjectPolicy.cs
using BoardGamesStore.Services.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;

public class SmtpClientPooledObjectPolicy : IPooledObjectPolicy<SmtpClient>
{
  private readonly SmtpSettings _smtpSettings;

  public SmtpClientPooledObjectPolicy(IOptions<SmtpSettings> smtpSettings)
  {
    _smtpSettings = smtpSettings.Value;
  }

  // Цей метод створює, підключає та автентифікує новий SmtpClient
  public SmtpClient Create()
  {
    var client = new SmtpClient();

    try
    {
      // 1. Отримуємо Access Token від Google
      var credential = new UserCredential(
          new GoogleAuthorizationCodeFlow(
              new GoogleAuthorizationCodeFlow.Initializer
              {
                ClientSecrets = new ClientSecrets
                {
                  ClientId = _smtpSettings.ClientId,
                  ClientSecret = _smtpSettings.ClientSecret
                }
              }),
          _smtpSettings.User, // User ID
          new TokenResponse { RefreshToken = _smtpSettings.RefreshToken }
      );

      // Асинхронно оновлюємо токен. Оскільки метод Create() синхронний,
      // ми змушені чекати на результат тут.
      // Це нормально, оскільки створення об'єкта для пулу - рідкісна операція.
      var success = credential.RefreshTokenAsync(CancellationToken.None).GetAwaiter().GetResult();
      if (!success || credential.Token == null || string.IsNullOrEmpty(credential.Token.AccessToken))
      {
        throw new InvalidOperationException("Failed to refresh Google OAuth2 token.");
      }

      var accessToken = credential.Token.AccessToken;

      // 2. Підключаємося до SMTP сервера
      var secureOption = _smtpSettings.StartTls
          ? SecureSocketOptions.StartTls
          : SecureSocketOptions.Auto;

      client.Connect(_smtpSettings.Host, _smtpSettings.Port, secureOption);

      // 3. Автентифікуємося за допомогою OAuth2
      var oauth2 = new SaslMechanismOAuth2(_smtpSettings.User, accessToken);
      client.Authenticate(oauth2);

      return client;
    }
    catch
    {
      // Якщо щось пішло не так на будь-якому етапі, знищуємо клієнт
      client.Dispose();
      throw;
    }
  }

  // Цей метод викликається, коли клієнт повертається в пул
  public bool Return(SmtpClient client)
  {
    // Перевіряємо, чи з'єднання все ще активне
    if (!client.IsConnected)
    {
      return false; // Якщо ні, пул його знищить і створить новий при потребі
    }
    return true;
  }
}