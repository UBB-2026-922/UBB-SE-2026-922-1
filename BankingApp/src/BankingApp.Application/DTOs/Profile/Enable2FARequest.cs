// <copyright file="Enable2FARequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the Enable2FARequest class.
// </summary>

using BankingApp.Application.Enums;

namespace BankingApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Represents a request to enable 2FA.
/// </summary>
public class Enable2FaRequest
{
    /// <summary>
    ///     Gets or sets the 2FA method to enable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TwoFactorMethod Method { get; set; }
}
