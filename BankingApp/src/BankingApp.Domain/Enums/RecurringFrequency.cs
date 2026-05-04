// <copyright file="RecurringFrequency.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringFrequency enum.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the execution frequency of a recurring payment.
/// </summary>
public enum RecurringFrequency
{
    /// <summary>
    ///     The payment executes once every day.
    /// </summary>
    Daily,

    /// <summary>
    ///     The payment executes once every week.
    /// </summary>
    Weekly,

    /// <summary>
    ///     The payment executes once every two weeks.
    /// </summary>
    BiWeekly,

    /// <summary>
    ///     The payment executes once every month.
    /// </summary>
    Monthly,

    /// <summary>
    ///     The payment executes once every three months.
    /// </summary>
    Quarterly,

    /// <summary>The payment runs once per year.</summary>
    Yearly,
}
