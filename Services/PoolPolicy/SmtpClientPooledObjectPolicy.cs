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

  public SmtpClient Create()
  {
    var client = new SmtpClient();

    try
    {
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
          _smtpSettings.User,
          new TokenResponse { RefreshToken = _smtpSettings.RefreshToken }
      );

      var success = credential.RefreshTokenAsync(CancellationToken.None).GetAwaiter().GetResult();
      if (!success || credential.Token == null || string.IsNullOrEmpty(credential.Token.AccessToken))
      {
        throw new InvalidOperationException("Failed to refresh Google OAuth2 token.");
      }

      var accessToken = credential.Token.AccessToken;

      var secureOption = _smtpSettings.StartTls
          ? SecureSocketOptions.StartTls
          : SecureSocketOptions.Auto;

      client.Connect(_smtpSettings.Host, _smtpSettings.Port, secureOption);

      var oauth2 = new SaslMechanismOAuth2(_smtpSettings.User, accessToken);
      client.Authenticate(oauth2);

      return client;
    }
    catch
    {
      client.Dispose();
      throw;
    }
  }

  public bool Return(SmtpClient client)
  {
    if (!client.IsConnected)
    {
      return false;
    }
    return true;
  }
}