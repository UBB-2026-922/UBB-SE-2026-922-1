// <copyright file="EmailService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the EmailService class.
// </summary>

using System.Net;
using System.Net.Mail;
using BankApp.Application.Services.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankApp.Infrastructure.Services.Notifications;

/// <summary>
///     Sends transactional emails using SMTP _configuration from application settings.
/// </summary>
public class EmailService : IEmailService
{
    private const int DefaultSmtpPort = 587;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailService" /> class.
    /// </summary>
    /// <param name="configuration">The application _configuration containing SMTP settings.</param>
    /// <param name="logger">Logger for email send failures.</param>
    /// <returns>The result of the operation.</returns>
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    public void SendLockNotification(string email)
    {
        SendEmail(email, EmailTemplates.AccountLockedSubject, EmailTemplates.AccountLockedBody);
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    public void SendLoginAlert(string email)
    {
        SendEmail(email, EmailTemplates.LoginAlertSubject, EmailTemplates.LoginAlertBody);
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    /// <param name="code">The code value.</param>
    public void SendOtpCode(string email, string code)
    {
        SendEmail(email, EmailTemplates.OtpSubject, EmailTemplates.GetOtpBody(code));
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    /// <param name="token">The token value.</param>
    public void SendPasswordResetLink(string email, string token)
    {
        SendEmail(email, EmailTemplates.PasswordResetSubject, EmailTemplates.GetPasswordResetBody(token));
    }

    private void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            string host = _configuration["Email:SmtpHost"] ??
                          throw new InvalidOperationException("Email:SmtpHost is missing from _configuration.");
            int port = int.Parse(
                _configuration["Email:SmtpPort"] ??
                throw new InvalidOperationException("Email:SmtpPort is missing from _configuration."));
            string smtpUsername = _configuration["Email:SmtpUser"] ??
                                  throw new InvalidOperationException("Email:SmtpUser is missing from _configuration.");
            string smtpPassword = _configuration["Email:SmtpPass"] ??
                                  throw new InvalidOperationException("Email:SmtpPass is missing from _configuration.");
            string fromAddress = _configuration["Email:FromAddress"] ?? smtpUsername;
            using var client = new SmtpClient(host, port);
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            using var mailMessage = new MailMessage(fromAddress, toEmail, subject, body);
            client.Send(mailMessage);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send email to {ToEmail} with subject '{Subject}'.",
                toEmail,
                subject);
        }
    }
}