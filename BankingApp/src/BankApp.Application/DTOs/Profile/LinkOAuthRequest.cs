// <copyright file="LinkOAuthRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the LinkOAuthRequest class.
// </summary>

namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Represents a request to link an OAuth provider to the current account.
/// </summary>
public class LinkOAuthRequest
{
    /// <summary>
    ///     Gets or sets the OAuth provider to link.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Provider { get; set; } = string.Empty;
}