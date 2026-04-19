// <copyright file="Session.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Session class.
// </summary>

namespace BankApp.Domain.Entities;

/// <summary>
///     Represents an active user session.
/// </summary>
public class Session
{
    /// <summary>
    ///     Gets or sets the unique identifier for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who owns this session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the authentication token for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the device information for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? DeviceInfo { get; set; }

    /// <summary>
    ///     Gets or sets the browser used for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Browser { get; set; }

    /// <summary>
    ///     Gets or sets the IP address from which the session was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? IpAddress { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the last activity in this session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the session expires.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the session has been revoked.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsRevoked { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the session was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}