// <copyright file="NotificationPreferenceDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationPreferenceDataTransferObject class.
// </summary>

using BankApp.Application.Enums;

namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Data transfer object representing a user's notification preference for a specific category.
/// </summary>
public class NotificationPreferenceDataTransferObject
{
    /// <summary>
    ///     Gets or sets the unique identifier of the preference.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user this preference belongs to.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the notification category this preference applies to.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public NotificationType Category { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether push notifications are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool PushEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether email notifications are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether SMS notifications are enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool SmsEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the minimum amount threshold that triggers a notification.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal? MinAmountThreshold { get; set; }
}
