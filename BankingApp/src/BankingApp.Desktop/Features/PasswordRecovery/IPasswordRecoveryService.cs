namespace BankingApp.Desktop.Features.PasswordRecovery;

using Shared.Enums;

/// <summary>Coordinates desktop password recovery state around the shared authentication API client.</summary>
public interface IPasswordRecoveryService
{
    /// <summary>Gets a value indicating whether a reset code can be requested.</summary>
    public bool CanResendCode { get; }

    /// <summary>Gets the remaining seconds until another reset code can be requested.</summary>
    public int SecondsUntilResendAllowed { get; }

    /// <summary>Starts the password-reset flow for the supplied email address.</summary>
    public Task<ForgotPasswordState> RequestCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Verifies whether the supplied reset token is still valid.</summary>
    public Task<ForgotPasswordState> VerifyTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Resets the password with the supplied token and new password.</summary>
    public Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a password satisfies desktop-side strength validation.</summary>
    public bool IsPasswordValid(string password);
}
