namespace BankingApp.Application.Features.UserProfile.Dtos;

using Domain.Entities;
using Domain.Enums;

/// <summary>
///     Represents the profile information of a user.
/// </summary>
public class ProfileDto
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileDto" /> class.
    /// </summary>
    public ProfileDto()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileDto" /> class
    ///     from a user entity.
    /// </summary>
    /// <param name="user">The user entity to extract profile info from.</param>
    public ProfileDto(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        UserId = user.Id;
        Email = user.Email;
        FullName = user.FullName;
        PhoneNumber = user.PhoneNumber;
        DateOfBirth = user.DateOfBirth;
        Address = user.Address;
        Nationality = user.Nationality;
        PreferredLanguage = user.PreferredLanguage;
        Is2FaEnabled = user.Is2FaEnabled;
        Preferred2FaMethod = user.Preferred2FaMethod;
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
    public TwoFactorMethod? Preferred2FaMethod { get; set; }
}
