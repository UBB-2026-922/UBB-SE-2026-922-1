namespace BankingApp.Application.DTOs.BillPayments;

/// <summary>
///     Represents the request payload for paying a bill.
/// </summary>
public class BillPayRequestDto
{
    /// <summary>
    ///     Gets or sets the source account identifier.
    /// </summary>
    /// <value>The source account identifier.</value>
    public int SourceAccountId { get; init; }

    /// <summary>
    ///     Gets or sets the biller identifier.
    /// </summary>
    /// <value>The biller identifier.</value>
    public int BillerId { get; init; }

    /// <summary>
    ///     Gets or sets the biller reference (account number, contract ID, etc.).
    /// </summary>
    /// <value>The biller reference.</value>
    public string BillerReference { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>The payment amount.</value>
    public decimal Amount { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether to pay the full outstanding balance.
    /// </summary>
    /// <value>Whether to pay in full.</value>
    public bool IsPayInFull { get; init; }

    /// <summary>
    ///     Gets or sets the optional 2FA token for high-value payments.
    /// </summary>
    /// <value>The 2FA token.</value>
    public string? TwoFaToken { get; init; }
}
