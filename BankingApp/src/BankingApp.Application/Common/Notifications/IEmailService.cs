namespace BankingApp.Application.Common.Notifications;

/// <summary>
///     Defines operations for sending transactional emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    ///     Sends a password reset link to the specified email address.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="token">The password reset token to include.</param>
    public void SendPasswordResetLink(string email, string token);

    /// <summary>
    ///     Sends an OTP code to the specified email address.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    /// <param name="code">The OTP code to send.</param>
    public void SendOtpCode(string email, string code);

    /// <summary>
    ///     Sends a login alert notification to the specified email address.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    public void SendLoginAlert(string email);

    /// <summary>
    ///     Sends an account lock notification to the specified email address.
    /// </summary>
    /// <param name="email">The recipient email address.</param>
    public void SendLockNotification(string email);
}
