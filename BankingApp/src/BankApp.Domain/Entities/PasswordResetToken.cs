// <copyright file="PasswordResetToken.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the PasswordResetToken class.
// </summary>

namespace BankApp.Domain.Entities;

/// <summary>
///     Represents a password reset token issued to a user.
/// </summary>
public class PasswordResetToken
{
    /// <summary>
    ///     Gets or sets the unique identifier for the token.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user this token belongs to.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the hashed value of the reset token.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the token expires.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the token was used, if applicable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the token was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}