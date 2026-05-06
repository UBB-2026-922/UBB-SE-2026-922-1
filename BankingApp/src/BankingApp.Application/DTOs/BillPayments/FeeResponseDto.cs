namespace BankingApp.Application.DTOs.BillPayments;

/// <summary>
///     Represents the fee calculation response.
/// </summary>
public class FeeResponseDto
{
    /// <summary>
    ///     Gets or sets the calculated fee.
    /// </summary>
    /// <value>The fee amount.</value>
    public decimal Fee { get; init; }
}
