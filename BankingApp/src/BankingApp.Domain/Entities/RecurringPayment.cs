// <copyright file="RecurringPayment.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPayment class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a scheduled, repeating bill payment associated with a user account.
/// </summary>
public class RecurringPayment
{
    /// <summary>Gets or sets the unique identifier of the recurring payment schedule.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the user who owns this schedule.</summary>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the biller (payee) for this schedule.</summary>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the identifier of the source account funds are drawn from.</summary>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the fixed payment amount per execution.</summary>
    public decimal Amount { get; set; }

    /// <summary>When <see langword="true" />, the full outstanding balance is paid instead of <see cref="Amount" />.</summary>
    public bool IsPayInFull { get; set; }

    /// <summary>Gets or sets how often the payment repeats.</summary>
    public RecurringFrequency Frequency { get; set; }

    /// <summary>Gets or sets the date on which the schedule begins executing.</summary>
    public DateTime StartDate { get; set; }

    /// <summary><see langword="null" /> means the schedule continues indefinitely.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the date on which the next payment will be executed.</summary>
    public DateTime NextExecutionDate { get; set; }

    /// <summary>Gets or sets the current lifecycle state of the schedule.</summary>
    public RecurringPaymentStatus Status { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the schedule was created.</summary>
    public DateTime CreatedAt { get; set; }
}