// <copyright file="RecurringPaymentStatus.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentStatus values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the lifecycle state of a recurring payment schedule.
/// </summary>
public enum RecurringPaymentStatus
{
    /// <summary>The schedule is active and will execute on its next scheduled date.</summary>
    Active,

    /// <summary>The schedule has been temporarily paused and will not execute until resumed.</summary>
    Paused,

    /// <summary>The schedule has been permanently cancelled.</summary>
    Cancelled,
}
