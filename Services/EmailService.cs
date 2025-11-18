// Services/EmailService.cs

using MailKit.Net.Smtp;
using BoardGamesStore.Services.Settings;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly ObjectPool<SmtpClient> _clientPool;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ObjectPool<SmtpClient> clientPool,
        IOptions<SmtpSettings> smtpSettings,
        ILogger<EmailService> logger)
    {
        _clientPool = clientPool;
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(_smtpSettings.From));
        mime.To.Add(MailboxAddress.Parse(email));
        mime.Subject = subject;
        mime.Body = new TextPart("html") { Text = message };

        var client = _clientPool.Get();

        try
        {
            _logger.LogInformation("Sending email to {Email}", email);
            await client.SendAsync(mime);
            _logger.LogInformation("Email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
        finally
        {
            _clientPool.Return(client);
        }
    }
}