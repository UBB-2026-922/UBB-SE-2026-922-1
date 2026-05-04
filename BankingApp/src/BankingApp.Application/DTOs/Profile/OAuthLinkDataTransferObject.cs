// <copyright file="OAuthLinkDataTransferObject.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the OAuthLinkDataTransferObject class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Data transfer object representing a linked OAuth provider for a user.
/// </summary>
public class OAuthLinkDataTransferObject
{
    /// <summary>
    ///     Gets or sets the unique identifier of the OAuth link.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the OAuth provider name (e.g. Google, Facebook).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Provider { get; set; } = string.Empty;

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