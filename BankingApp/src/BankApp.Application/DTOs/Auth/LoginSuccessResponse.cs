// <copyright file="LoginSuccessResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for LoginSuccessResponse.
// </summary>

namespace BankApp.Application.DataTransferObjects.Auth;

/// <summary>
///     The JSON response body returned by a successful login or OTP-verification endpoint.
///     When <see cref="Requires2Fa" /> is <see langword="true" />, <see cref="Token" /> is
///     <see langword="null" /> and the client must complete the two-factor flow before a
///     token is issued.
/// </summary>
public sealed class LoginSuccessResponse
{
    /// <summary>
    ///     Gets or sets the identifier of the authenticated user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the signed JWT for subsequent authenticated requests,
    ///     or <see langword="null" /> when <see cref="Requires2Fa" /> is <see langword="true" />.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Token { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user must complete a two-factor
    ///     authentication step before a token is issued.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool Requires2Fa { get; set; }
}