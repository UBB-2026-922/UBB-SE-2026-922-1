// <copyright file="BillPayment.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPayment entity for the bill payment feature.
// </summary>

using System;
using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
/// Represents a one-off bill payment made by a user to a registered biller.
/// </summary>
public class BillPayment
{
    /// <summary>
    /// Gets or sets the unique identifier for this bill payment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who initiated the payment.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this payment.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the source account from which the funds are drawn.
    /// </summary>
    public int SourceAccountId { get; set; }

    /// <summary>
    /// Gets or sets the source account associated with this payment.
    /// </summary>
    public Account? SourceAccount { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the biller receiving the payment.
    /// </summary>
    public int BillerId { get; set; }

    /// <summary>
    /// Gets or sets the biller associated with this payment.
    /// </summary>
    public Biller? Biller { get; set; }

    /// <summary>
    /// Gets or sets the optional identifier of the overarching transaction.
    /// </summary>
    public int? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the overarching transaction associated with this payment.
    /// </summary>
    public Transaction? Transaction { get; set; }

    /// <summary>
    /// Gets or sets the reference code or invoice number provided by the biller.
    /// </summary>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount paid to the biller.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the processing fee charged for this payment.
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// Gets or sets the unique receipt number generated for this payment.
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the bill payment.
    /// </summary>
    public BillPaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
