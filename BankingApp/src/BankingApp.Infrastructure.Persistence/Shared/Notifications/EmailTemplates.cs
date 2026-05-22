namespace BankingApp.Infrastructure.Common.Notifications;

/// <summary>
///     Defines the subjects and body templates for all transactional emails sent by the application.
/// </summary>
public static class EmailTemplates
{
    /// <summary>Subject for the account-locked notification.</summary>
    public const string AccountLockedSubject = "BankingApp - Account Locked";

    /// <summary>Body for the account-locked notification.</summary>
    public const string AccountLockedBody =
        "Hello,\n\nYour account has been temporarily locked due to multiple failed login attempts. " +
        "Please try again later or reset your password.";

    /// <summary>Subject for the new-login alert.</summary>
    public const string LoginAlertSubject = "BankingApp - New Login Detected";

    /// <summary>Body for the new-login alert.</summary>
    public const string LoginAlertBody =
        "Hello,\n\nWe detected a new login to your BankingApp account. " +
        "If this was you, no action is needed. " +
        "If this wasn't you, please change your password immediately.";

    /// <summary>Subject for the password-reset email.</summary>
    public const string PasswordResetSubject = "BankingApp - Password Reset Code";

    /// <summary>
    ///     Returns the body for the password-reset email, embedding the raw <paramref name="token" />.
    /// </summary>
    /// <param name="token">The password-reset token to include in the message.</param>
    /// <returns>The formatted email body.</returns>
    public static string GetPasswordResetBody(string token)
    {
        return $"Hello,\n\nYou requested a password reset. " +
               $"Please copy and paste the recovery code below into the application:\n\n{token}\n\n" +
               "If you did not request this, please ignore this email.";
    }
}
