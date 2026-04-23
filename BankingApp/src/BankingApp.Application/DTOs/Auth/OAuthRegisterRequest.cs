// <copyright file="OAuthRegisterRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the OAuthRegisterRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Auth;

/// <summary>
///     Represents a registration request using an OAuth provider token.
/// </summary>
public class OAuthRegisterRequest
{
    /// <summary>
    ///     Gets or sets the OAuth provider name (e.g. Google, Facebook).
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the token received from the OAuth provider.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string ProviderToken { get; set; } = string.Empty;

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
}