// <copyright file="ProfileViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ProfileViewModel class.
// </summary>

using System;
using System.Threading.Tasks;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Application.Enums;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
///     Coordinates profile-related operations by delegating to specialised sub-ViewModels
///     for personal info, security, OAuth, notifications, and sessions.
/// </summary>
public class ProfileViewModel
{
    private readonly ILogger<ProfileViewModel> _logger;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileViewModel" /> class.
    /// </summary>
    /// <param name="personalInfo">The personal info sub-ViewModel.</param>
    /// <param name="security">The security sub-ViewModel.</param>
    /// <param name="oauthViewModel">The OAuth sub-ViewModel.</param>
    /// <param name="notifications">The notifications sub-ViewModel.</param>
    /// <param name="sessions">The sessions sub-ViewModel.</param>
    /// <param name="logger">Logger for profile coordination errors.</param>
    public ProfileViewModel(
        PersonalInfoViewModel personalInfo,
        SecurityViewModel security,
        OAuthViewModel oauthViewModel,
        NotificationsViewModel notifications,
        SessionsViewModel sessions,
        ILogger<ProfileViewModel> logger)
    {
        PersonalInfo = personalInfo ?? throw new ArgumentNullException(nameof(personalInfo));
        Security = security ?? throw new ArgumentNullException(nameof(security));
        OAuth = oauthViewModel ?? throw new ArgumentNullException(nameof(oauthViewModel));
        Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
    }

    /// <summary>
    ///     Gets the current profile workflow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the View is currently initializing UI controls
    ///     programmatically and toggle-changed events should be suppressed.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsInitializingView { get; set; }

    /// <summary>
    ///     Gets the personal info sub-ViewModel.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public PersonalInfoViewModel PersonalInfo { get; }

    /// <summary>
    ///     Gets the security sub-ViewModel.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public SecurityViewModel Security { get; }

    /// <summary>
    ///     Gets the OAuth sub-ViewModel.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public OAuthViewModel OAuth { get; }

    /// <summary>
    ///     Gets the notifications sub-ViewModel.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public NotificationsViewModel Notifications { get; }

    /// <summary>
    ///     Gets the sessions sub-ViewModel.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public SessionsViewModel Sessions { get; }

    /// <summary>
    ///     Gets the current user's profile details (convenience accessor).
    /// </summary>
    /// <value>
    ///     The current user's profile details (convenience accessor).
    /// </value>
    public ProfileInfo ProfileInfo => PersonalInfo.ProfileInfo;

    /// <summary>
    ///     Gets a value indicating whether phone-based 2FA is active.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsPhoneTwoFactorActive =>
        ProfileInfo.Is2FaEnabled && ProfileInfo.Preferred2FaMethod == TwoFactorMethod.Phone;

    /// <summary>
    ///     Gets a value indicating whether email-based 2FA is active.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsEmailTwoFactorActive =>
        ProfileInfo.Is2FaEnabled && ProfileInfo.Preferred2FaMethod == TwoFactorMethod.Email;

    /// <summary>
    ///     Loads the current user's profile, OAuth links, and notification preferences.
    ///     Each request is issued sequentially; the load stops at the first failure.
    /// </summary>
    /// <returns><see langword="true" /> if all data loaded successfully; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LoadProfile()
    {
        State.SetValue(ProfileState.Loading);
        if (!await PersonalInfo.LoadProfile())
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        if (!await OAuth.LoadOAuthLinks())
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        if (!await Notifications.LoadNotificationPreferences())
        {
            State.SetValue(ProfileState.Error);
            return false;
        }

        State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

    /// <summary>
    ///     Enables 2FA and updates the local profile state when successful.
    /// </summary>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns><see langword="true" /> if 2FA was enabled; otherwise, <see langword="false" />.</returns>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        bool success = await Security.EnableTwoFactor(method);
        if (!success)
        {
            return false;
        }

        ProfileInfo.Is2FaEnabled = true;
        ProfileInfo.Preferred2FaMethod = method;
        return true;
    }

    /// <summary>
    ///     Disables 2FA and updates the local profile state when successful.
    /// </summary>
    /// <returns><see langword="true" /> if 2FA was disabled; otherwise, <see langword="false" />.</returns>
    public async Task<bool> DisableTwoFactor()
    {
        bool success = await Security.DisableTwoFactor();
        if (!success)
        {
            return false;
        }

        ProfileInfo.Is2FaEnabled = false;
        ProfileInfo.Preferred2FaMethod = null;
        return true;
    }

    /// <summary>
    ///     Sets email 2FA from the profile toggle and updates local state.
    /// </summary>
    /// <param name="enabled"><see langword="true" /> to enable email 2FA; otherwise, disable it.</param>
    /// <returns><see langword="true" /> if the setting was updated; otherwise, <see langword="false" />.</returns>
    public async Task<bool> SetEmailTwoFactorEnabled(bool enabled)
    {
        bool success = await Security.SetTwoFactorEnabled(enabled);
        if (!success)
        {
            return false;
        }

        ProfileInfo.Is2FaEnabled = enabled;
        ProfileInfo.Preferred2FaMethod = enabled ? TwoFactorMethod.Email : null;
        return true;
    }

    /// <summary>
    ///     Toggles a notification preference and lets the notification model roll back on failure.
    /// </summary>
    /// <param name="preference">The preference to toggle.</param>
    /// <param name="enabled">The new enabled value.</param>
    /// <returns><see langword="true" /> if the preference was saved; otherwise, <see langword="false" />.</returns>
    public Task<bool> ToggleNotificationPreference(NotificationPreferenceDataTransferObject preference, bool enabled)
    {
        return Notifications.ToggleNotificationPreference(preference, enabled);
    }

    /// <summary>
    ///     Loads sessions for the currently loaded user.
    /// </summary>
    /// <returns>A result indicating whether sessions were loaded and why loading may have failed.</returns>
    public async Task<(bool Success, string? ErrorMessage)> LoadSessionsForCurrentUser()
    {
        int? userId = ProfileInfo.UserId;
        if (userId == null)
        {
            return (false, "User not loaded.");
        }

        bool loaded = await Sessions.LoadSessionsAsync(userId.Value);
        return loaded ? (true, null) : (false, "Failed to load active sessions.");
    }

    /// <summary>
    ///     Revokes a session and reloads the current user's active sessions.
    /// </summary>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns>A result indicating whether the revoke and reload flow completed.</returns>
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

    /// <summary>
    ///     Releases resources used by the view model.
    /// </summary>
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
