// <copyright file="BillPayment.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPayment entity for Team B's bill payment feature.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a one-off bill payment made by a user to a registered biller.
///     Maps to the SQL table <c>BillPayment</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One):
///         every payment is owned by exactly one user.
///     </para>
///     <para>
///         Reuses the base entity <see cref="Account" /> via <see cref="SourceAccountId" /> (Many-to-One):
///         the debit account must exist in the <c>Account</c> table.
///     </para>
///     <para>
///         Optionally references the base entity <see cref="Transaction" /> via <see cref="TransactionId" />
///         (One-to-One, nullable): when the payment is processed, the resulting ledger entry is linked here.
///     </para>
///     <para>
///         References <see cref="Biller" /> via <see cref="BillerId" /> (Many-to-One):
///         a payment must target a registered biller.
///     </para>
/// </remarks>
public class BillPayment
{
    /// <summary>
    ///     Gets or sets the unique identifier for this bill payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the <see cref="User" /> who made this payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the source <see cref="Account" /> debited for this payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int SourceAccountId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the <see cref="Biller" /> that receives this payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int BillerId { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the linked <see cref="Transaction" /> ledger entry,
    ///     or <see langword="null" /> when the payment has not yet been executed.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? TransactionId { get; set; }

    /// <summary>
    ///     Gets or sets the biller-specific reference number (e.g., invoice number or contract ID).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the amount paid to the biller.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the processing fee charged for this payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Fee { get; set; }

    /// <summary>
    ///     Gets or sets the unique receipt number issued upon payment confirmation.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current processing status of the payment.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public BillPaymentStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) when this payment was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}
