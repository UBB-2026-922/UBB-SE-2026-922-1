namespace BankingApp.Domain.Entities;

using Enums;
using ErrorOr;
using Errors;

/// <summary>
/// Represents a transfer initiated by a user between accounts or to an external recipient.
/// </summary>
public class Transfer
{
    private const decimal TwoFaAmountThreshold = 1000m;
    private const int ExpectedCurrencyCodeLength = 3;
    private const int IbanMinLength = 15;
    private const int IbanMaxLength = 34;
    private const int IbanCountryCodeLength = 2;

    /// <summary>
    /// Gets or sets the transfer identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user who initiated the transfer.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the source account for the transfer.
    /// </summary>
    public Account? SourceAccount { get; set; }

    /// <summary>
    /// Gets or sets the related ledger transaction, if any.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <summary>
    /// Gets or sets the recipient's name.
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient's IBAN.
    /// </summary>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient bank name, if available.
    /// </summary>
    public string? RecipientBankName { get; set; }

    /// <summary>
    /// Gets or sets the amount to transfer.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency of the transfer amount.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount after currency conversion, if applicable.
    /// </summary>
    public decimal? ConvertedAmount { get; set; }

    /// <summary>
    /// Gets or sets the exchange rate used for conversion, if applicable.
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Gets or sets the fee charged for the transfer.
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// Gets or sets an optional reference for the transfer.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the current status of the transfer.
    /// </summary>
    public TransferStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the estimated arrival date and time for the transfer, if known.
    /// </summary>
    public DateTime? EstimatedArrival { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the transfer.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Validates core transfer rules that belong to the transfer domain model.
    /// </summary>
    /// <param name="recipientIban">The recipient IBAN.</param>
    /// <param name="amount">The transfer amount.</param>
    /// <param name="currency">The transfer currency code.</param>
    /// <returns>The result of the operation.</returns>
    public static ErrorOr<Success> Validate(string recipientIban, decimal amount, string currency)
    {
        if (!IsValidRecipientIban(recipientIban))
        {
            return TransferErrors.InvalidIban;
        }

        if (amount <= 0)
        {
            return TransferErrors.InvalidAmount;
        }

        if (!IsValidCurrency(currency))
        {
            return TransferErrors.InvalidCurrency;
        }

        return Result.Success;
    }

    /// <summary>
    ///     Returns true when the amount requires two-factor authentication.
    /// </summary>
    /// <param name="amount">The transfer amount.</param>
    /// <returns>The result of the operation.</returns>
    public static bool RequiresTwoFactorAuthentication(decimal amount)
    {
        return amount >= TwoFaAmountThreshold;
    }

    /// <summary>
    ///     Performs basic structural validation of the recipient IBAN.
    /// </summary>
    /// <param name="iban">The IBAN to validate.</param>
    /// <returns>The result of the operation.</returns>
    public static bool IsValidRecipientIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        if (iban.Length is < IbanMinLength or > IbanMaxLength)
        {
            return false;
        }

        if (!char.IsLetter(iban[0]) || !char.IsLetter(iban[1]))
        {
            return false;
        }

        if (!char.IsDigit(iban[2]) || !char.IsDigit(iban[3]))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Infers a display bank name from the IBAN country code.
    /// </summary>
    /// <param name="iban">The recipient IBAN.</param>
    /// <returns>The inferred bank name.</returns>
    public static string InferRecipientBankName(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban) || iban.Length < IbanCountryCodeLength)
        {
            return "Unknown Bank";
        }

        return iban[..IbanCountryCodeLength].ToUpperInvariant() switch
        {
            "RO" => "Romanian Bank",
            "DE" => "German Bank",
            "GB" => "UK Bank",
            "FR" => "French Bank",
            "US" => "US Bank",
            _ => "International Bank"
        };
    }

    private static bool IsValidCurrency(string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) && currency.Length == ExpectedCurrencyCodeLength;
    }
}
