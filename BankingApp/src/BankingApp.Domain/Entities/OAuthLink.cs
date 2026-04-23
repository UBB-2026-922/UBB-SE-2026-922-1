// <copyright file="OAuthLink.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the OAuthLink class.
// </summary>

namespace BankingApp.Domain.Entities;

/// <summary>
///     Represents an OAuth provider link for a user account.
/// </summary>
public class OAuthLink
{
    /// <summary>
    ///     Gets or sets the unique identifier for the OAuth link.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who owns this link.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the OAuth provider name (e.g. Google, Facebook).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the user identifier from the OAuth provider.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address associated with the OAuth provider account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? ProviderEmail { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the OAuth link was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime LinkedAt { get; set; }
}