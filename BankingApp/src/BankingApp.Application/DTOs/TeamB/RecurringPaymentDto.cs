// <copyright file="RecurringPaymentDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentDto class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries recurring payment schedule data between the application and presentation layers.
/// </summary>
public class RecurringPaymentDto
{
    /// <summary>Gets or sets the unique identifier (0 for new schedules).</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning user's identifier.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the target biller's identifier.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the target biller's display name.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the source account's identifier.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the amount debited on each execution.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets a value indicating whether the full outstanding balance is paid instead of a fixed amount.</summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsPayInFull { get; set; }

    /// <summary>Gets or sets the execution cadence.</summary>
    /// <value>Gets or sets the current value.</value>
    public RecurringFrequency Frequency { get; set; }

    /// <summary>Gets or sets the first scheduled execution date.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the last allowed execution date, or <see langword="null" /> for indefinite.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the next scheduled execution date.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime NextExecutionDate { get; set; }

    /// <summary>Gets or sets the current lifecycle status.</summary>
    /// <value>Gets or sets the current value.</value>
    public RecurringPaymentStatus Status { get; set; }
}