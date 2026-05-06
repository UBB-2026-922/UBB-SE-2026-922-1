using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Handles loading and updating the user's personal profile information.
/// </summary>
public partial class PersonalInfoViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<PersonalInfoViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PersonalInfoViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for profile operations.</param>
    /// <param name="logger">Logger for personal info operation errors.</param>
    /// <returns>The result of the operation.</returns>
    public PersonalInfoViewModel(IApiClient apiClient, ILogger<PersonalInfoViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        ProfileDto = new ProfileDto();
    }

    /// <summary>
    ///     Gets the current profile workflow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets the current user's profile details.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ProfileDto ProfileDto { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the user has a phone number on file.
    /// </summary>
    /// <value>
    ///     A value indicating whether the user has a phone number on file.
    /// </value>
    public bool HasPhoneNumber => !string.IsNullOrEmpty(ProfileDto.PhoneNumber);

    /// <summary>
    ///     Gets the display text for the two-factor phone field.
    ///     Returns a placeholder when no phone number has been set.
    /// </summary>
    /// <value>
    ///     The display text for the two-factor phone field.
    ///     Returns a placeholder when no phone number has been set.
    /// </value>
    public string TwoFactorPhoneDisplay =>
        HasPhoneNumber ? ProfileDto.PhoneNumber! : UserMessages.Profile.NoPhoneNumber;

    /// <summary>
    ///     Loads the current user's profile information from the server.
    /// </summary>
    /// <returns><see langword="true" /> if the profile loaded successfully; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LoadProfile()
    {
        State.SetValue(ProfileState.Loading);
        ErrorOr<ProfileDto> profileResult = await _apiClient.GetAsync<ProfileDto>(ApiEndpoints.Profile);
        if (profileResult.IsError)
        {
            _logger.LoadProfileFailed(profileResult.Errors);
            State.SetValue(ProfileState.Error);
            return false;
        }

        ProfileDto = profileResult.Value ?? new ProfileDto();
        State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

    /// <summary>
    ///     Updates the user's phone number and address.
    /// </summary>
    /// <param name="phone">The phone number to persist.</param>
    /// <param name="address">The address to persist.</param>
    /// <param name="password">The verified password associated with the edit flow.</param>
    /// <param name="fullName">The full name to persist, or <see langword="null" /> to keep the current value.</param>
    /// <returns><see langword="true" /> if the update succeeded; otherwise, <see langword="false" />.</returns>
    public async Task<bool> UpdatePersonalInfo(string? phone, string? address, string password, string? fullName = null)
    {
        State.SetValue(ProfileState.Loading);
        if (ProfileDto.UserId == null)
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        string? trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        string? trimmedFullName = string.IsNullOrWhiteSpace(fullName) ? ProfileDto.FullName : fullName.Trim();
        var request = new UpdateProfileRequest
        {
            UserId = ProfileDto.UserId,
            FullName = trimmedFullName,
            PhoneNumber = trimmedPhone,
            DateOfBirth = ProfileDto.DateOfBirth,
            Address = trimmedAddress,
            Nationality = ProfileDto.Nationality,
            PreferredLanguage = ProfileDto.PreferredLanguage
        };
        ErrorOr<Success> result = await _apiClient.PutAsync(ApiEndpoints.Profile, request);
        return result.Match(
            _ =>
            {
                ProfileDto.FullName = trimmedFullName;
                ProfileDto.PhoneNumber = trimmedPhone;
                ProfileDto.Address = trimmedAddress;
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
    ///     Verifies the supplied password against the server.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <returns><see langword="true" /> if the password is valid; otherwise, <see langword="false" />.</returns>
    public async Task<bool> VerifyPassword(string password)
    {
        State.SetValue(ProfileState.Loading);
        if (ProfileDto.UserId == null)
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        ErrorOr<bool> result = await _apiClient.PostAsync<string, bool>(ApiEndpoints.VerifyPassword, password);
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
