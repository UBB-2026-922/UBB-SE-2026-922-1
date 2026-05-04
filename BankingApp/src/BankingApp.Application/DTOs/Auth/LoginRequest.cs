// <copyright file="LoginRequest.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the LoginRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Auth;

/// <summary>
///     Represents a login request with email and password credentials.
/// </summary>
public class LoginRequest
{
    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Password { get; set; } = string.Empty;
}