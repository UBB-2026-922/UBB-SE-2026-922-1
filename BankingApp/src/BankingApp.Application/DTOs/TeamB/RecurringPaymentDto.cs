// <copyright file="RecurringPaymentDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentDto record.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries recurring payment schedule data between the application and presentation layers.
/// </summary>
/// <param name="Id">The unique identifier (0 for new schedules).</param>
/// <param name="UserId">The owning user's identifier.</param>
/// <param name="BillerId">The target biller's identifier.</param>
/// <param name="BillerName">The target biller's display name.</param>
/// <param name="SourceAccountId">The source account's identifier.</param>
/// <param name="Amount">The amount debited on each execution.</param>
/// <param name="IsPayInFull">Whether the full outstanding balance is paid instead of the fixed amount.</param>
/// <param name="Frequency">The execution cadence.</param>
/// <param name="StartDate">The first scheduled execution date.</param>
/// <param name="EndDate">The last allowed execution date, or null for indefinite.</param>
/// <param name="NextExecutionDate">The next scheduled execution date.</param>
/// <param name="Status">The current lifecycle status.</param>
public record RecurringPaymentDto(
    int Id,
    int UserId,
    int BillerId,
    string BillerName,
    int SourceAccountId,
    decimal Amount,
    bool IsPayInFull,
    RecurringFrequency Frequency,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime NextExecutionDate,
    RecurringPaymentStatus Status);
