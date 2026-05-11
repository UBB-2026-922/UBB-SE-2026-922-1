namespace BankingApp.Application.Features.UserProfile.Dtos;

using Domain.Enums;

/// <summary>
///     Represents a request to enable two-factor authentication.
/// </summary>
public class EnableTwoFaRequest
{
    /// <summary>
    ///     Gets or sets the two-factor authentication method to enable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TwoFactorMethod Method { get; set; }
}
