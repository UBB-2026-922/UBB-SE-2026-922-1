namespace BankingApp.Application.DTOs.BillPayments;

/// <summary>
///     Represents a user account for bill payment source selection.
/// </summary>
public class AccountDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    /// <value>The account identifier.</value>
    public int Id { get; init; }

    /// <summary>
    ///     Gets or sets the account IBAN.
    /// </summary>
    /// <value>The IBAN.</value>
    public string Iban { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account currency.
    /// </summary>
    /// <value>The currency code.</value>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account balance.
    /// </summary>
    /// <value>The account balance.</value>
    public decimal Balance { get; init; }

    /// <summary>
    ///     Gets or sets the account display name.
    /// </summary>
    /// <value>The account name.</value>
    public string AccountName { get; init; } = string.Empty;
}
