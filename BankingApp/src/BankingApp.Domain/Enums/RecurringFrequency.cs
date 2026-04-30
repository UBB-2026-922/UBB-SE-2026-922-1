// <copyright file="RecurringFrequency.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
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
    ///     The payment executes once every week.
    /// </summary>
    Weekly,

    /// <summary>
    ///     The payment executes once every month.
    /// </summary>
    Monthly,

    /// <summary>
    ///     The payment executes once every three months.
    /// </summary>
    Quarterly,
}
