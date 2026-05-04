// <copyright file="ForgotPasswordRequest.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ForgotPasswordRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Auth;

/// <summary>
///     Represents a request to initiate the forgot-password flow.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    ///     Gets or sets the email address to send the reset link to.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;
}