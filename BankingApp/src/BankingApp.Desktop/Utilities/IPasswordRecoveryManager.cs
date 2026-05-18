namespace BankingApp.Desktop.Utilities;

using System.Threading.Tasks;
using Enums;

/// <summary>Encapsulates the password-recovery flow and resend throttling.</summary>
public interface IPasswordRecoveryManager
{
    /// <summary>Gets a value indicating whether a new recovery code can be requested now.</summary>
    public bool CanResendCode { get; }

    /// <summary>Gets the remaining cooldown in seconds before another code can be requested.</summary>
    public int SecondsUntilResendAllowed { get; }

    /// <summary>Requests a recovery code for the specified email address.</summary>
    public Task<ForgotPasswordState> RequestCodeAsync(string email);

    /// <summary>Verifies the supplied reset token.</summary>
    public Task<ForgotPasswordState> VerifyTokenAsync(string token);

    /// <summary>Resets the password using a verified reset token.</summary>
    public Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword);

    /// <summary>Checks whether the password satisfies the policy.</summary>
    public bool IsPasswordValid(string password);
}
