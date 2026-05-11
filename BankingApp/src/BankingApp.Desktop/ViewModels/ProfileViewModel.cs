namespace BankingApp.Desktop.ViewModels;

using System;
using System.Threading.Tasks;
using Application.Features.UserProfile.Dtos;
using Enums;
using BankingApp.Domain.Enums;

/// <summary>
///     Coordinates profile-related operations by delegating to specialized sub-ViewModels
///     for personal info, security, notifications, and sessions.
/// </summary>
public partial class ProfileViewModel : ObservableObject, IDisposable
{
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="ProfileViewModel"/> class.</summary>
    public ProfileViewModel(
        PersonalInfoViewModel personalInfo,
        SecurityViewModel security,
        NotificationsViewModel notifications,
        SessionsViewModel sessions)
    {
        PersonalInfo = personalInfo ?? throw new ArgumentNullException(nameof(personalInfo));
        Security = security ?? throw new ArgumentNullException(nameof(security));
        Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
    }

    /// <summary>Gets or sets the current profile workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Gets or sets a value indicating whether the View is currently initializing controls programmatically.</summary>
    public bool IsInitializingView { get; set; }

    /// <summary>Gets the personal info sub-ViewModel.</summary>
    public PersonalInfoViewModel PersonalInfo { get; }

    /// <summary>Gets the security sub-ViewModel.</summary>
    public SecurityViewModel Security { get; }

    /// <summary>Gets the notifications sub-ViewModel.</summary>
    public NotificationsViewModel Notifications { get; }

    /// <summary>Gets the sessions sub-ViewModel.</summary>
    public SessionsViewModel Sessions { get; }

    /// <summary>Gets the current user's profile details (convenience accessor).</summary>
    public ProfileDto ProfileDto => PersonalInfo.ProfileDto;

    /// <summary>Gets a value indicating whether phone-based 2FA is active.</summary>
    public bool IsPhoneTwoFactorActive =>
        ProfileDto is { Is2FaEnabled: true, Preferred2FaMethod: TwoFactorMethod.Phone };

    /// <summary>Gets a value indicating whether email-based 2FA is active.</summary>
    public bool IsEmailTwoFactorActive =>
        ProfileDto is { Is2FaEnabled: true, Preferred2FaMethod: TwoFactorMethod.Email };

    /// <summary>Loads the current user's profile, OAuth links, and notification preferences.</summary>
    public async Task<bool> LoadProfile()
    {
        State = ProfileState.Loading;
        if (!await PersonalInfo.LoadProfile() || !await Notifications.LoadNotificationPreferences())
        {
            State = ProfileState.Error;
            return false;
        }

        State = ProfileState.UpdateSuccess;
        return true;
    }

    /// <summary>Enables 2FA and updates the local profile state when successful.</summary>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        bool success = await Security.EnableTwoFactor(method);
        if (!success)
        {
            return false;
        }

        ProfileDto.Is2FaEnabled = true;
        ProfileDto.Preferred2FaMethod = method;
        return true;
    }

    /// <summary>Disables 2FA and updates the local profile state when successful.</summary>
    public async Task<bool> DisableTwoFactor()
    {
        bool success = await Security.DisableTwoFactor();
        if (!success)
        {
            return false;
        }

        ProfileDto.Is2FaEnabled = false;
        ProfileDto.Preferred2FaMethod = null;
        return true;
    }

    /// <summary>Sets email 2FA from the profile toggle and updates local state.</summary>
    public async Task<bool> SetEmailTwoFactorEnabled(bool enabled)
    {
        bool success = await Security.SetTwoFactorEnabled(enabled);
        if (!success)
        {
            return false;
        }

        ProfileDto.Is2FaEnabled = enabled;
        ProfileDto.Preferred2FaMethod = enabled ? TwoFactorMethod.Email : null;
        return true;
    }

    /// <summary>Toggles a notification preference and lets the notification model roll back on failure.</summary>
    public Task<bool> ToggleNotificationPreference(NotificationPreferenceDto preference, bool enabled) =>
        Notifications.ToggleNotificationPreference(preference, enabled);

    /// <summary>Loads sessions for the currently loaded user.</summary>
    public async Task<(bool Success, string? ErrorMessage)> LoadSessionsForCurrentUser()
    {
        int? userId = ProfileDto.UserId;
        if (userId == null)
        {
            return (false, "User not loaded.");
        }

        bool loaded = await Sessions.LoadSessionsAsync(userId.Value);
        return loaded ? (true, null) : (false, "Failed to load active sessions.");
    }

    /// <summary>Revokes a session and reloads the current user's active sessions.</summary>
    public async Task<(bool Success, string? ErrorMessage)> RevokeSessionAndReload(int sessionId)
    {
        bool revoked = await Sessions.RevokeSessionAsync(sessionId);
        if (!revoked)
        {
            return (false, "Failed to revoke session.");
        }

        (bool loaded, string? errorMessage) = await LoadSessionsForCurrentUser();
        return loaded ? (true, null) : (false, errorMessage);
    }

    /// <summary>Releases resources used by the view model.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
