namespace BankingApp.Contracts.Features.Authentication.Dtos;

/// <summary>
///     Represents a request to verify an OTP during two-factor authentication.
/// </summary>
public class VerifyOtpRequest
{
    /// <summary>
    ///     Gets or sets the identifier of the user verifying the OTP.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the OTP code to verify.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string OtpCode { get; set; } = string.Empty;
}