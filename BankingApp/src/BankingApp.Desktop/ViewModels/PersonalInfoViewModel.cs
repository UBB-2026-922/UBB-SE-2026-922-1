namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using BankingApp.Application.Features.UserProfile.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Handles personal-profile loading, editing, and password verification for the profile area.
/// </summary>
public class PersonalInfoViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<PersonalInfoViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PersonalInfoViewModel" /> class.
    /// </summary>
    public PersonalInfoViewModel(IProfileClientService profileClientService, ILogger<PersonalInfoViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        ProfileInfo = new ProfileDto();
    }

    /// <summary>
    ///     Gets the current profile workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets the loaded profile details.
    /// </summary>
    public ProfileDto ProfileInfo { get; private set; }

    /// <summary>
    ///     Gets the loaded profile details using the legacy property name expected elsewhere in the UI.
    /// </summary>
    public ProfileDto ProfileDto => ProfileInfo;

    /// <summary>
    ///     Gets a value indicating whether the user has a phone number on file.
    /// </summary>
    public bool HasPhoneNumber => !string.IsNullOrEmpty(ProfileInfo.PhoneNumber);

    /// <summary>
    ///     Gets the phone-number text shown in the two-factor section.
    /// </summary>
    public string TwoFactorPhoneDisplay =>
        HasPhoneNumber ? ProfileInfo.PhoneNumber! : UserMessages.Profile.NoPhoneNumber;

    /// <summary>
    ///     Loads the current user's profile.
    /// </summary>
    public async Task<bool> LoadProfile()
    {
        State.SetValue(ProfileState.Loading);
        ErrorOr<ProfileDto> profileResult = await _profileClientService.GetProfileAsync();
        if (profileResult.IsError)
        {
            _logger.LoadProfileFailed(profileResult.Errors);
            State.SetValue(ProfileState.Error);
            return false;
        }

        ProfileInfo = profileResult.Value;
        State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

    /// <summary>
    ///     Updates editable personal-information fields for the current user.
    /// </summary>
    public async Task<bool> UpdatePersonalInfo(string? phone, string? address, string password, string? fullName = null)
    {
        State.SetValue(ProfileState.Loading);
        if (ProfileInfo.UserId == null)
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        string? trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        string? trimmedFullName = string.IsNullOrWhiteSpace(fullName) ? ProfileInfo.FullName : fullName.Trim();
        var request = new UpdateProfileRequest
        {
            UserId = ProfileInfo.UserId,
            FullName = trimmedFullName,
            PhoneNumber = trimmedPhone,
            DateOfBirth = ProfileInfo.DateOfBirth,
            Address = trimmedAddress,
            Nationality = ProfileInfo.Nationality,
            PreferredLanguage = ProfileInfo.PreferredLanguage,
        };
        ErrorOr<Success> result = await _profileClientService.UpdateProfileAsync(request);
        return result.Match(
            _ =>
            {
                ProfileInfo.FullName = trimmedFullName;
                ProfileInfo.PhoneNumber = trimmedPhone;
                ProfileInfo.Address = trimmedAddress;
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.UpdatePersonalInfoFailed(errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    ///     Verifies the current password against the server.
    /// </summary>
    public async Task<bool> VerifyPassword(string password)
    {
        State.SetValue(ProfileState.Loading);
        if (ProfileInfo.UserId == null)
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        ErrorOr<bool> result = await _profileClientService.VerifyPasswordAsync(password);
        return result.Match(
            valid =>
            {
                if (!valid)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.VerifyPasswordFailed(errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }
}
