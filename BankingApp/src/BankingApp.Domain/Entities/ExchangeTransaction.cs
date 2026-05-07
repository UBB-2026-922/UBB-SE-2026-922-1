namespace BankingApp.Domain.Entities;

using Enums;

/// <summary>
///     Represents a foreign-exchange conversion between two of a user's accounts.
///     Maps to the SQL table <c>ExchangeTransaction</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One).
///     </para>
///     <para>
///         Reuses the base entity <see cref="Account" /> twice:
///         via <see cref="SourceAccountId" /> (the account debited in the source currency)
///         and via <see cref="TargetAccountId" /> (the account credited in the target currency).
///         Both foreign keys use <c>DeleteBehavior.NoAction</c> to avoid cascade conflicts.
///     </para>
///     <para>
///         Optionally references the base entity <see cref="Transaction" /> via <see cref="TransactionId" />
///         (One-to-One, nullable): linked after the exchange is executed in the ledger.
///     </para>
/// </remarks>
public class ExchangeTransaction
{
    /// <summary>Gets or sets the unique identifier for this exchange transaction.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the <see cref="User" /> who initiated this exchange.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the user who initiated this exchange.</summary>
    public User? User { get; set; }

    /// <summary>Gets or sets the identifier of the source <see cref="Account" /> debited in the source currency.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the source account debited in the source currency.</summary>
    public Account? SourceAccount { get; set; }

    /// <summary>Gets or sets the identifier of the target <see cref="Account" /> credited in the target currency.</summary>
    /// <value>Gets or sets the current value.</value>
    public int TargetAccountId { get; set; }

    /// <summary>Gets or sets the target account credited in the target currency.</summary>
    public Account? TargetAccount { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the linked <see cref="Transaction" /> ledger entry,
    ///     or <see langword="null" /> when the exchange has not yet been executed.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public int? TransactionId { get; set; }

    /// <summary>
    ///     Gets or sets the linked ledger transaction, or <see langword="null" /> when none exists.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <summary>Gets or sets the ISO 4217 currency code of the source account (e.g., "EUR").</summary>
    /// <value>Gets or sets the current value.</value>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 4217 currency code of the target account (e.g., "USD").</summary>
    /// <value>Gets or sets the current value.</value>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount deducted from the source account.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal SourceAmount { get; set; }

    /// <summary>Gets or sets the amount credited to the target account after commission.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TargetAmount { get; set; }

    /// <summary>Gets or sets the exchange rate applied at the time of execution.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the commission charged by the bank for this conversion.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Commission { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) when the exchange rate was locked,
    ///     or <see langword="null" /> if no rate was pre-locked.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime? RateLockedAt { get; set; }

    /// <summary>Gets or sets the current processing status of this exchange.</summary>
    /// <value>Gets or sets the current value.</value>
    public ExchangeTransactionStatus Status { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when this exchange was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}
