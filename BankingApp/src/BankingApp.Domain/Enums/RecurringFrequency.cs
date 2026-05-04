// <copyright file="RecurringFrequency.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringFrequency values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents how often a recurring payment is executed.
/// </summary>
public enum RecurringFrequency
{
    /// <summary>The payment runs every day.</summary>
    Daily,

    /// <summary>The payment runs once per week.</summary>
    Weekly,

    /// <summary>The payment runs every two weeks.</summary>
    BiWeekly,

    /// <summary>The payment runs once per month.</summary>
    Monthly,

    /// <summary>The payment runs once every three months.</summary>
    Quarterly,

    /// <summary>The payment runs once per year.</summary>
    Yearly,
}
