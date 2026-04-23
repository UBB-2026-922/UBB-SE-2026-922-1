// <copyright file="RegisterRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RegisterRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Auth;

/// <summary>
///     Represents a registration request with email, password and user details.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    ///     Gets or sets the email address for the new account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password for the new account.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the full name of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string FullName { get; set; } = string.Empty;
}