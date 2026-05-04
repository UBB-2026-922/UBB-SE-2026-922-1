// <copyright file="Transfer.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the Transfer entity for Team B's bank transfer feature.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents an outbound bank transfer initiated by a user to a recipient IBAN.
///     Maps to the SQL table <c>Transfers</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One):
///         every transfer is owned by exactly one user.
///     </para>
///     <para>
///         Reuses the base entity <see cref="Account" /> via <see cref="SourceAccountId" /> (Many-to-One):
///         the debit account must exist in the <c>Account</c> table.
///     </para>
///     <para>
///         Optionally references the base entity <see cref="Transaction" /> via <see cref="TransactionId" />
///         (One-to-One, nullable): when the transfer is processed, the resulting ledger entry is linked here.
///     </para>
/// </remarks>
public class Transfer
{
    /// <summary>
    ///     Gets or sets the unique identifier for this transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the <see cref="User" /> who initiated this transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the source <see cref="Account" /> debited for this transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int SourceAccountId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the linked <see cref="Transaction" /> ledger entry,
    ///     or <see langword="null" /> when the transfer has not yet been executed.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? TransactionId { get; set; }

    /// <summary>
    ///     Gets or sets the full name of the transfer recipient.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the International Bank Account Number of the recipient.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the recipient's bank, if known.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? RecipientBankName { get; set; }

    /// <summary>
    ///     Gets or sets the amount sent in the source currency.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the ISO 4217 currency code for the transfer amount.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the amount received by the recipient after currency conversion,
    ///     or <see langword="null" /> for same-currency transfers.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? ConvertedAmount { get; set; }

    /// <summary>
    ///     Gets or sets the exchange rate applied during the transfer,
    ///     or <see langword="null" /> for same-currency transfers.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    ///     Gets or sets the processing fee charged for this transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Fee { get; set; }

    /// <summary>
    ///     Gets or sets the free-text reference message included with the transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Reference { get; set; }

    /// <summary>
    ///     Gets or sets the current processing status of the transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransferStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets the estimated arrival date and time at the recipient bank,
    ///     or <see langword="null" /> if not yet determined.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? EstimatedArrival { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) when this transfer was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}
