// <copyright file="SessionMetadata.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the SessionMetadata class.
// </summary>

namespace BankApp.Application.Services.Login;

/// <summary>
///     Describes request-derived metadata stored with a login session.
/// </summary>
public class SessionMetadata
{
    /// <summary>
    ///     Gets or sets the device or user-agent information for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? DeviceInfo { get; set; }

    /// <summary>
    ///     Gets or sets the browser detected for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Browser { get; set; }

    /// <summary>
    ///     Gets or sets the originating IP address for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? IpAddress { get; set; }
}