// <copyright file="NotificationsViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationsViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Handles loading and updating notification preferences for the current user.
/// </summary>
public class NotificationsViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<NotificationsViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationsViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for notification operations.</param>
    /// <param name="logger">Logger for notification operation errors.</param>
    public NotificationsViewModel(IApiClient apiClient, ILogger<NotificationsViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        NotificationPreferences = new List<NotificationPreferenceDataTransferObject>();
    }

    /// <summary>
    ///     Gets the current notifications workflow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets the notification preferences for the current user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<NotificationPreferenceDataTransferObject> NotificationPreferences { get; private set; }

    /// <summary>
    ///     Toggles one notification preference and saves the updated list.
    /// </summary>
    /// <param name="preference">The preference to update.</param>
    /// <param name="enabled">Whether email notifications should be enabled.</param>
    /// <returns><see langword="true" /> if the update was saved; otherwise, <see langword="false" />.</returns>
    public async Task<bool> ToggleNotificationPreference(
        NotificationPreferenceDataTransferObject preference,
        bool enabled)
    {
        bool previousValue = preference.EmailEnabled;
        preference.EmailEnabled = enabled;
        bool success = await UpdateNotificationPreferences(NotificationPreferences);
        if (!success)
        {
            preference.EmailEnabled = previousValue;
        }

        return success;
    }

    /// <summary>
    ///     Loads notification preferences for the current user from the server.
    /// </summary>
    /// <returns><see langword="true" /> if loaded successfully; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LoadNotificationPreferences()
    {
        ErrorOr<List<NotificationPreferenceDataTransferObject>> preferencesResult =
            await _apiClient.GetAsync<List<NotificationPreferenceDataTransferObject>>(
                ApiEndpoints.NotificationPreferences);
        if (preferencesResult.IsError)
        {
            _logger.LogError("LoadNotificationPreferences: request failed: {Errors}", preferencesResult.Errors);
            return false;
        }

        NotificationPreferences = preferencesResult.Value;
        return true;
    }

    /// <summary>
    ///     Updates notification preferences for the current user.
    /// </summary>
    /// <param name="preferences">The preferences to persist.</param>
    /// <returns><see langword="true" /> if the preferences were updated; otherwise, <see langword="false" />.</returns>
    public async Task<bool> UpdateNotificationPreferences(List<NotificationPreferenceDataTransferObject> preferences)
    {
        if (preferences.Count == default)
        {
            return false;
        }

        State.SetValue(ProfileState.Loading);
        ErrorOr<Success> result = await _apiClient.PutAsync(ApiEndpoints.NotificationPreferences, preferences);
        return result.Match(
            _ =>
            {
                NotificationPreferences = preferences;
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.LogError("UpdateNotificationPreferences failed: {Errors}", errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }
}