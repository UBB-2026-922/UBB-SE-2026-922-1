// <copyright file="NotificationTypeExtensions.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationTypeExtensions class.
// </summary>

namespace BankingApp.Application.Enums;

/// <summary>
///     Provides display helpers for the application-facing <see cref="NotificationType" /> enum.
/// </summary>
public static class NotificationTypeExtensions
{
    /// <summary>
    ///     Converts a notification category to the display text shown in presentation layers.
    /// </summary>
    /// <param name="type">The notification category to format.</param>
    /// <returns>The human-readable display name.</returns>
    public static string ToDisplayName(this NotificationType type)
    {
        return type switch
        {
            NotificationType.Payment => "Payment",
            NotificationType.InboundTransfer => "Inbound Transfer",
            NotificationType.OutboundTransfer => "Outbound Transfer",
            NotificationType.LowBalance => "Low Balance",
            NotificationType.DuePayment => "Due Payment",
            NotificationType.SuspiciousActivity => "Suspicious Activity",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
