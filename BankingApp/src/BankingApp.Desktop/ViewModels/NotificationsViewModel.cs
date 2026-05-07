namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Profile;
using Enums;
using Services;
using Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Handles notification-preference loading and updates for the profile area.
/// </summary>
public class NotificationsViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<NotificationsViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationsViewModel" /> class.
    /// </summary>
    public NotificationsViewModel(IProfileClientService profileClientService, ILogger<NotificationsViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        NotificationPreferences = new List<NotificationPreferenceDto>();
    }

    /// <summary>
    ///     Gets the current notifications workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets the current notification preferences.
    /// </summary>
    public List<NotificationPreferenceDto> NotificationPreferences { get; private set; }

    /// <summary>
    ///     Toggles a single notification preference and rolls the change back if persistence fails.
    /// </summary>
    public async Task<bool> ToggleNotificationPreference(NotificationPreferenceDto preference, bool enabled)
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
    ///     Loads notification preferences for the current user.
    /// </summary>
    public async Task<bool> LoadNotificationPreferences()
    {
        ErrorOr<List<NotificationPreferenceDto>> preferencesResult =
            await _profileClientService.GetNotificationPreferencesAsync();
        if (preferencesResult.IsError)
        {
            _logger.LoadNotificationPreferencesFailed(preferencesResult.Errors);
            return false;
        }

        NotificationPreferences = preferencesResult.Value;
        return true;
    }

    /// <summary>
    ///     Persists the provided notification preferences.
    /// </summary>
    public async Task<bool> UpdateNotificationPreferences(List<NotificationPreferenceDto> preferences)
    {
        if (preferences.Count == 0)
        {
            return false;
        }

        State.SetValue(ProfileState.Loading);
        ErrorOr<Success> result = await _profileClientService.UpdateNotificationPreferencesAsync(preferences);
        return result.Match(
            _ =>
            {
                NotificationPreferences = preferences;
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.UpdateNotificationPreferencesFailed(errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }
}
