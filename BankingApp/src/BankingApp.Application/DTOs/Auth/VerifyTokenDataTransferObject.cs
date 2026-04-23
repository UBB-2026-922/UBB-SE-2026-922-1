// <copyright file="VerifyTokenDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the VerifyTokenDataTransferObject class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Auth;

/// <summary>
///     Data transfer object used for reset token verification requests.
/// </summary>
public class VerifyTokenDataTransferObject
{
    /// <summary>
    ///     Gets or sets the reset token to be verified.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Token { get; set; } = string.Empty;
}