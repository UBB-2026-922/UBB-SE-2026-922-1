namespace BankingApp.Contracts.Features.BillPayments.Dtos;

/// <summary>
///     Represents the 2FA requirement check response.
/// </summary>
public class RequiresTwoFaResponse
{
    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is required.
    /// </summary>
    /// <value>Whether 2FA is required.</value>
    public bool Required { get; set; }
}
