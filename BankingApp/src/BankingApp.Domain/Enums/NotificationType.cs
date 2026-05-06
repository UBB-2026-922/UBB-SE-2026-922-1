// <copyright file="NotificationType.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationType values.
// </summary>

namespace BankingApp.Domain.Enums;

/// <summary>
///     Represents the category of a user notification.
/// </summary>
public enum NotificationType
{
    /// <summary>
    ///     A payment was made from the account.
    /// </summary>
    Payment,

    /// <summary>
    ///     Funds were transferred out of the account.
    /// </summary>
    OutboundTransfer,

    /// <summary>
    ///     Funds were received into the account.
    /// </summary>
    InboundTransfer,

    /// <summary>
    ///     The account balance has dropped below a configured threshold.
    /// </summary>
    LowBalance,

    /// <summary>
    ///     A payment is due soon on the account.
    /// </summary>
    DuePayment,

    /// <summary>
    ///     Unusual activity has been detected on the account.
    /// </summary>
    SuspiciousActivity,
}