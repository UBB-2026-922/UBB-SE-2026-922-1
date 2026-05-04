// <copyright file="CreateRecurringPaymentRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the CreateRecurringPaymentRequest class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.RecurringPayments;

/// <summary>
///     Request payload for creating a new recurring payment schedule.
/// </summary>
public class CreateRecurringPaymentRequest
{
    public int BillerId { get; set; }
    public int SourceAccountId { get; set; }
    public decimal Amount { get; set; }

    /// <summary>When <see langword="true" />, the full outstanding balance is paid instead of <see cref="Amount" />.</summary>
    public bool IsPayInFull { get; set; }

    public RecurringFrequency Frequency { get; set; }
    public DateTime StartDate { get; set; }

    /// <summary><see langword="null" /> means the schedule continues indefinitely.</summary>
    public DateTime? EndDate { get; set; }
}
