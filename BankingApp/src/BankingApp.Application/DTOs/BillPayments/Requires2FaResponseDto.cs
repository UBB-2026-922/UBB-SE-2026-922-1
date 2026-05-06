namespace BankingApp.Application.DTOs.BillPayments;

/// <summary>
///     Represents the 2FA requirement check response.
/// </summary>
public class Requires2FaResponseDto
{
    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is required.
    /// </summary>
    /// <value>Whether 2FA is required.</value>
    public bool Required { get; init; }
}
