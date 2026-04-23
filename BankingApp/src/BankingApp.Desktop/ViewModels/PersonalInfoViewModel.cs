// <copyright file="PersonalInfoViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the PersonalInfoViewModel class.
// </summary>

using System;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Handles loading and updating the user's personal profile information.
/// </summary>
public class PersonalInfoViewModel
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
        ProfileInfo = new ProfileInfo();
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
    public ProfileInfo ProfileInfo { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the user has a phone number on file.
    /// </summary>
    /// <value>
    ///     A value indicating whether the user has a phone number on file.
    /// </value>
    public bool HasPhoneNumber => !string.IsNullOrEmpty(ProfileInfo.PhoneNumber);

    /// <summary>
    ///     Gets the display text for the two-factor phone field.
    ///     Returns a placeholder when no phone number has been set.
    /// </summary>
    /// <value>
    ///     The display text for the two-factor phone field.
    ///     Returns a placeholder when no phone number has been set.
    /// </value>
    public string TwoFactorPhoneDisplay =>
        HasPhoneNumber ? ProfileInfo.PhoneNumber! : UserMessages.Profile.NoPhoneNumber;

    /// <summary>
    ///     Loads the current user's profile information from the server.
    /// </summary>
    /// <returns><see langword="true" /> if the profile loaded successfully; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LoadProfile()
    {
        State.SetValue(ProfileState.Loading);
        ErrorOr<ProfileInfo> profileResult = await _apiClient.GetAsync<ProfileInfo>(ApiEndpoints.Profile);
        if (profileResult.IsError)
        {
            _logger.LogError("LoadProfile: profile request failed: {Errors}", profileResult.Errors);
            State.SetValue(ProfileState.Error);
            return false;
        }

        ProfileInfo = profileResult.Value;
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
        if (ProfileInfo.UserId == null)
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        string? trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        string? trimmedFullName = string.IsNullOrWhiteSpace(fullName) ? ProfileInfo.FullName : fullName.Trim();
        var request = new UpdateProfileRequest(ProfileInfo.UserId, trimmedPhone, trimmedAddress)
        {
            FullName = trimmedFullName,
            DateOfBirth = ProfileInfo.DateOfBirth,
            Nationality = ProfileInfo.Nationality,
            PreferredLanguage = ProfileInfo.PreferredLanguage,
        };
        ErrorOr<Success> result = await _apiClient.PutAsync(ApiEndpoints.Profile, request);
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
                _logger.LogError("UpdatePersonalInfo failed: {Errors}", errors);
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
        if (ProfileInfo.UserId == null)
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
                _logger.LogError("VerifyPassword failed: {Errors}", errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }
}