// <copyright file="EnableTwoFaRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the EnableTwoFaRequest class.
// </summary>

using BankApp.Application.Enums;

namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Represents a request to enable two-factor authentication.
/// </summary>
public class EnableTwoFaRequest
{
    /// <summary>
    ///     Gets or sets the two-factor authentication method to enable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TwoFactorMethod Method { get; set; }
}
