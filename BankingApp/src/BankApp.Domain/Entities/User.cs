// <copyright file="User.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the User class.
// </summary>

using BankApp.Domain.Enums;

namespace BankApp.Domain.Entities;

/// <summary>
///     Represents a user of the banking application.
/// </summary>
public class User
{
    /// <summary>
    ///     The default preferred language assigned to new users.
    /// </summary>
    public const string DefaultPreferredLanguage = "en";

    /// <summary>
    ///     Gets or sets the unique identifier for the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the hashed password of the user.
    ///     <see langword="null" /> for OAuth-only accounts that have no password set.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PasswordHash { get; set; }

    /// <summary>
    ///     Gets or sets the full name of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the phone number of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets the date of birth of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    ///     Gets or sets the address of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Address { get; set; }

    /// <summary>
    ///     Gets or sets the nationality of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Nationality { get; set; }

    /// <summary>
    ///     Gets or sets the preferred language of the user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string PreferredLanguage { get; set; } = DefaultPreferredLanguage;

    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool Is2FaEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the preferred 2FA method.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TwoFactorMethod? Preferred2FaMethod { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user account is locked.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsLocked { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the lockout ends, if applicable.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    ///     Gets or sets the number of consecutive failed login attempts.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the user was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the user was last updated.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     Returns true when the account lockout is still in effect at the current UTC time.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public bool IsCurrentlyLocked()
    {
        return IsLocked && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    /// <summary>
    ///     Enables two-factor authentication for the specified delivery method.
    /// </summary>
    /// <param name="method">The two-factor authentication method to use.</param>
    public void Enable2Fa(TwoFactorMethod method)
    {
        Is2FaEnabled = true;
        Preferred2FaMethod = method;
    }

    /// <summary>
    ///     Disables two-factor authentication and clears the preferred method.
    /// </summary>
    public void Disable2Fa()
    {
        Is2FaEnabled = false;
        Preferred2FaMethod = null;
    }
}