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
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BillerId { get; set; }
    public int SourceAccountId { get; set; }
    public decimal Amount { get; set; }

    /// <summary>When <see langword="true" />, the full outstanding balance is paid instead of <see cref="Amount" />.</summary>
    public bool IsPayInFull { get; set; }

    public RecurringFrequency Frequency { get; set; }
    public DateTime StartDate { get; set; }

    /// <summary><see langword="null" /> means the schedule continues indefinitely.</summary>
    public DateTime? EndDate { get; set; }

    public DateTime NextExecutionDate { get; set; }
    public RecurringPaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
