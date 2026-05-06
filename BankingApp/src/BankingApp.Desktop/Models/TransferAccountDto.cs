namespace BankingApp.Desktop.Models;

/// <summary>
///     Represents a bank account available for transfers, as returned by the API.
/// </summary>
public partial class TransferAccountDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the account IBAN.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account currency code (e.g. EUR, USD, RON).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current account balance.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string AccountName { get; set; } = string.Empty;
}
