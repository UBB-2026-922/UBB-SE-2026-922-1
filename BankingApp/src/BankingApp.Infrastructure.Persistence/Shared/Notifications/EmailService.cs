namespace BankingApp.Infrastructure.Common.Notifications;

using Application.Common.Notifications;
using Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

/// <summary>
///     Sends transactional emails using MailKit over SMTP.
/// </summary>
public sealed class EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = options.Value;

    /// <inheritdoc />
    public Task SendLoginAlertAsync(string email) =>
        SendAsync(email, EmailTemplates.LoginAlertSubject, EmailTemplates.LoginAlertBody);

    /// <inheritdoc />
    public Task SendPasswordResetLinkAsync(string email, string rawToken) =>
        SendAsync(email, EmailTemplates.PasswordResetSubject, EmailTemplates.GetPasswordResetBody(rawToken));

    public Task SendLockNotificationAsync(string email) =>
        SendAsync(email, EmailTemplates.AccountLockedSubject, EmailTemplates.AccountLockedBody);

    private async Task SendAsync(string toEmail, string subject, string body)
    {
        try
        {
            MimeMessage message = new();
            string from = _settings.FromAddress.Length > 0 ? _settings.FromAddress : _settings.SmtpUser;
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using SmtpClient client = new();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
        }
        catch (Exception exception)
        {
            logger.EmailSendFailed(exception, toEmail, subject);
        }
    }
}
