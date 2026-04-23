// <copyright file="UpdateProfileRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the UpdateProfileRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Profile;

/// <summary>
///     Represents a request to update user profile fields.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateProfileRequest" /> class.
    /// </summary>
    public UpdateProfileRequest()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateProfileRequest" /> class.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="phoneNumber">The new phone number.</param>
    /// <param name="address">The new address.</param>
    public UpdateProfileRequest(int? userId, string? phoneNumber, string? address)
    {
        UserId = userId;
        PhoneNumber = phoneNumber;
        Address = address;
    }

    /// <summary>
    ///     Gets or sets the identifier of the user whose profile is being updated.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int? UserId { get; set; }

    /// <summary>
    ///     Gets or sets the new full name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? FullName { get; set; }

    /// <summary>
    ///     Gets or sets the new phone number.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Gets or sets the new date of birth.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    ///     Gets or sets the new address.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Address { get; set; }

    /// <summary>
    ///     Gets or sets the new nationality.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Nationality { get; set; }

    /// <summary>
    ///     Gets or sets the new preferred language.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? PreferredLanguage { get; set; }
}