namespace BankingApp.Contracts.Features.PasswordReset.Dtos;

/// <summary>
///     Data transfer object used for reset token verification requests.
/// </summary>
public class VerifyResetTokenRequest
{
    /// <summary>
    ///     Gets or sets the reset token to be verified.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Token { get; set; } = string.Empty;
}