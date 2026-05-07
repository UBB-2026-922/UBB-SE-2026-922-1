using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class NotificationsViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<NotificationsViewModel> _logger;

    public NotificationsViewModel(IProfileClientService profileClientService, ILogger<NotificationsViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        NotificationPreferences = new List<NotificationPreferenceDto>();
    }

    public ObservableState<ProfileState> State { get; }

    public List<NotificationPreferenceDto> NotificationPreferences { get; private set; }

    public async Task<bool> ToggleNotificationPreference(
        NotificationPreferenceDto preference,
        bool enabled)
    {
        bool previousValue = preference.EmailEnabled;
        preference.EmailEnabled = enabled;
        bool success = await UpdateNotificationPreferences(NotificationPreferences);
        if (!success) preference.EmailEnabled = previousValue;

        return success;
    }

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

    public async Task<bool> UpdateNotificationPreferences(List<NotificationPreferenceDto> preferences)
    {
        if (preferences.Count == default) return false;

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
