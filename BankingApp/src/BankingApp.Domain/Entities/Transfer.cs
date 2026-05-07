// <copyright file="Transfer.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the Transfer class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
/// Represents a transfer initiated by a user between accounts or to an external recipient.
/// </summary>
public class Transfer
{
    /// <summary>
    /// Gets or sets the transfer identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who initiated the transfer.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the source account for the transfer.
    /// </summary>
    public int SourceAccountId { get; set; }

    /// <summary>
    /// Gets or sets the related transaction identifier, if any.
    /// </summary>
    public int? TransactionId { get; set; }

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
}