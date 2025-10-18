// Services/EmailService.cs

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        // Тут буде ваша реальна логіка відправки пошти (наприклад, через SMTP або API)
        _logger.LogInformation("--- НОВИЙ ЛИСТ ---");
        _logger.LogInformation("Кому: {Email}", email);
        _logger.LogInformation("Тема: {Subject}", subject);
        _logger.LogInformation("Тіло: {Message}", message);
        _logger.LogInformation("--- КІНЕЦЬ ЛИСТА ---");

        return Task.CompletedTask;
    }
}