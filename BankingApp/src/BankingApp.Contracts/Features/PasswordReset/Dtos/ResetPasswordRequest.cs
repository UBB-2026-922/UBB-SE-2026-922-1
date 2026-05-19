namespace BankingApp.Contracts.Features.PasswordReset.Dtos;

/// <summary>
///     Represents a request to reset a password using a reset token.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    ///     Gets or sets the reset token.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the new password.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string NewPassword { get; set; } = string.Empty;
}