// <copyright file="UserSummaryDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the UserSummaryDataTransferObject class.
// </summary>

namespace BankApp.Application.DataTransferObjects.Dashboard;

/// <summary>
///     Data transfer object containing the user information shown on the dashboard.
/// </summary>
public class UserSummaryDataTransferObject
{
    /// <summary>
    ///     Gets or sets the full name of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the phone number of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool Is2FaEnabled { get; set; }
}