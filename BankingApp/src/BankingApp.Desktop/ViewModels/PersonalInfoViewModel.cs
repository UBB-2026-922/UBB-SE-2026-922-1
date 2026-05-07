using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class PersonalInfoViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<PersonalInfoViewModel> _logger;

    public PersonalInfoViewModel(IProfileClientService profileClientService, ILogger<PersonalInfoViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        ProfileInfo = new ProfileDto();
    }

    public ObservableState<ProfileState> State { get; }

    public ProfileDto ProfileInfo { get; private set; }

    public bool HasPhoneNumber => !string.IsNullOrEmpty(ProfileInfo.PhoneNumber);

    public string TwoFactorPhoneDisplay =>
        HasPhoneNumber ? ProfileInfo.PhoneNumber! : UserMessages.Profile.NoPhoneNumber;

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

        ProfileInfo = profileResult.Value ?? new ProfileDto();
        State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

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
