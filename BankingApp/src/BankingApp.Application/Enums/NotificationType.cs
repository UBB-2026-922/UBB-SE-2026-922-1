// <copyright file="NotificationType.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationType values.
// </summary>

namespace BankingApp.Application.Enums;

/// <summary>
///     Defines the application-facing notification categories exposed to presentation clients.
/// </summary>
public enum NotificationType
{
    /// <summary>
    ///     Payment notifications.
    /// </summary>
    Payment = 0,

    /// <summary>
    ///     Inbound transfer notifications.
    /// </summary>
    InboundTransfer = 1,

    /// <summary>
    ///     Outbound transfer notifications.
    /// </summary>
    OutboundTransfer = 2,

    /// <summary>
    ///     Low balance notifications.
    /// </summary>
    LowBalance = 3,

    /// <summary>
    ///     Due payment notifications.
    /// </summary>
    DuePayment = 4,

    /// <summary>
    ///     Suspicious activity notifications.
    /// </summary>
    SuspiciousActivity = 5,
}
