// <copyright file="Transaction.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Transaction class.
// </summary>

using BankApp.Domain.Enums;

namespace BankApp.Domain.Entities;

/// <summary>
///     Represents a financial transaction on an account.
/// </summary>
public class Transaction
{
    /// <summary>
    ///     Gets or sets the unique identifier for the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the account this transaction belongs to.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int AccountId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the card used for this transaction, if any.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? CardId { get; set; }

    /// <summary>
    ///     Gets or sets the unique reference code for the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string TransactionRef { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the direction of the transaction as a <see cref="TransactionDirection" />.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransactionDirection Direction { get; set; }

    /// <summary>
    ///     Gets or sets the amount of the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the currency code for the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account balance after this transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    ///     Gets or sets the name of the counterparty in the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? CounterpartyName { get; set; }

    /// <summary>
    ///     Gets or sets the IBAN of the counterparty.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? CounterpartyIban { get; set; }

    /// <summary>
    ///     Gets or sets the merchant name for card transactions.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? MerchantName { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the transaction category.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? CategoryId { get; set; }

    /// <summary>
    ///     Gets or sets the description of the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the fee charged for the transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Fee { get; set; }

    /// <summary>
    ///     Gets or sets the exchange rate applied to the transaction, if applicable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    ///     Gets or sets the status of the transaction as a <see cref="TransactionStatus" />.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransactionStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets the type of entity related to this transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the entity related to this transaction.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the transaction was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}