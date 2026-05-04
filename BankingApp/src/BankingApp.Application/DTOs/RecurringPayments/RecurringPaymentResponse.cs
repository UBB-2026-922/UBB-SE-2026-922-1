// <copyright file="RecurringPaymentResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentResponse class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.RecurringPayments;

/// <summary>
///     Response payload representing a recurring payment schedule.
/// </summary>
public class RecurringPaymentResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BillerId { get; set; }
    public int SourceAccountId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPayInFull { get; set; }
    public RecurringFrequency Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime NextExecutionDate { get; set; }
    public RecurringPaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
