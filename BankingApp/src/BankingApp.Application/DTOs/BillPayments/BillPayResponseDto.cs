namespace BankingApp.Application.DTOs.BillPayments;

/// <summary>
///     Represents the response returned after a successful bill payment.
/// </summary>
public class BillPayResponseDto
{
    /// <summary>
    ///     Gets or sets the payment identifier.
    /// </summary>
    /// <value>The payment identifier.</value>
    public int Id { get; init; }

    /// <summary>
    ///     Gets or sets the receipt number.
    /// </summary>
    /// <value>The receipt number.</value>
    public string ReceiptNumber { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the fee charged.
    /// </summary>
    /// <value>The fee amount.</value>
    public decimal Fee { get; init; }

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>The payment amount.</value>
    public decimal Amount { get; init; }

    /// <summary>
    ///     Gets or sets the payment status.
    /// </summary>
    /// <value>The payment status.</value>
    public string Status { get; init; } = string.Empty;
}
