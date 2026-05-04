// <copyright file="RecurringPayment.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPayment entity for Team B's recurring payment feature.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents a scheduled recurring payment from a user's account to a biller.
///     Maps to the SQL table <c>RecurringPayment</c> introduced by Team B.
/// </summary>
/// <remarks>
///     <para>
///         Reuses the base entity <see cref="User" /> via <see cref="UserId" /> (Many-to-One).
///     </para>
///     <para>
///         Reuses the base entity <see cref="Account" /> via <see cref="SourceAccountId" /> (Many-to-One).
///     </para>
///     <para>
///         References <see cref="Biller" /> via <see cref="BillerId" /> (Many-to-One).
///     </para>
/// </remarks>
public class RecurringPayment
{
    /// <summary>Gets or sets the unique identifier for this recurring payment.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the <see cref="User" /> who owns this schedule.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the target <see cref="Biller" />.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the identifier of the source <see cref="Account" /> to be debited.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the amount debited on each execution.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the full outstanding balance
    ///     is paid on each execution rather than a fixed amount.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public bool IsPayInFull { get; set; }

    /// <summary>Gets or sets the cadence at which this payment repeats.</summary>
    /// <value>Gets or sets the current value.</value>
    public RecurringFrequency Frequency { get; set; }

    /// <summary>Gets or sets the date and time (UTC) when the first payment is scheduled.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time (UTC) after which no further payments will be made,
    ///     or <see langword="null" /> for indefinite schedules.
    /// </summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime? EndDate { get; set; }

    /// <summary>Gets or sets the date and time (UTC) of the next scheduled execution.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime NextExecutionDate { get; set; }

    /// <summary>Gets or sets the current lifecycle status of this recurring payment.</summary>
    /// <value>Gets or sets the current value.</value>
    public RecurringPaymentStatus Status { get; set; } = RecurringPaymentStatus.Active;

    /// <summary>Gets or sets the date and time (UTC) when this schedule was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}
