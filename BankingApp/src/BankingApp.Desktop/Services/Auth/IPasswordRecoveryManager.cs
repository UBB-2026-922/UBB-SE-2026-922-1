namespace BankingApp.Desktop.Utilities;

using System.Threading.Tasks;
using Enums;

/// <summary>
///     Manages the business logic for the password-recovery flow, including
///     state transitions, token validation, and resend throttling.
/// </summary>
public interface IPasswordRecoveryManager
{
    /// <summary>Gets a value indicating whether the user is allowed to request a new recovery code.</summary>
    public bool CanResendCode { get; }

    /// <summary>Gets the number of seconds remaining before the user is allowed to resend a recovery code.</summary>
    public int SecondsUntilResendAllowed { get; }

    /// <summary>Requests a password-recovery code for the given email address.</summary>
    public Task<ForgotPasswordState> RequestCodeAsync(string email);

    /// <summary>Validates the supplied recovery token without consuming it.</summary>
    public Task<ForgotPasswordState> VerifyTokenAsync(string token);

    /// <summary>Resets the user's password using a previously verified token.</summary>
    public Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword);

    /// <summary>Validates a plain-text password against the application's complexity rules.</summary>
    public bool IsPasswordValid(string password);
}
