// <copyright file="ProfileInfo.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ProfileInfo class.
// </summary>

using BankingApp.Application.Mapping;
using BankingApp.Domain.Entities;

namespace BankingApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Represents the profile information of a user.
/// </summary>
public class ProfileInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileInfo" /> class.
    /// </summary>
    public ProfileInfo()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileInfo" /> class
    ///     from a user entity.
    /// </summary>
    /// <param name="user">The user entity to extract profile info from.</param>
    public ProfileInfo(User user)
    {
        if (user != null)
        {
            UserId = user.Id;
            Email = user.Email;
            FullName = user.FullName;
            PhoneNumber = user.PhoneNumber;
            DateOfBirth = user.DateOfBirth;
            Address = user.Address;
            Nationality = user.Nationality;
            PreferredLanguage = user.PreferredLanguage;
            Is2FaEnabled = user.Is2FaEnabled;
            Preferred2FaMethod = DomainEnumMapper.ToApplication(user.Preferred2FaMethod);
        }
    }

    /// <summary>
    ///     Gets or sets the user identifier.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? UserId { get; set; }

    /// <summary>
    ///     Gets or sets the email address.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Email { get; set; }

    /// <summary>
    ///     Gets or sets the full name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? FullName { get; set; }

    /// <summary>
    ///     Gets or sets the phone number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets the date of birth.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    ///     Gets or sets the address.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Address { get; set; }

    /// <summary>
    ///     Gets or sets the nationality.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Nationality { get; set; }

    /// <summary>
    ///     Gets or sets the preferred language.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is enabled.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool Is2FaEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the preferred 2FA delivery method.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public Enums.TwoFactorMethod? Preferred2FaMethod { get; set; }
}
