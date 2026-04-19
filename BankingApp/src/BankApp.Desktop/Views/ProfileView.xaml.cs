// <copyright file="ProfileView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for ProfileView.xaml.
// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Application.Enums;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Master;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace BankApp.Desktop.Views;

/// <summary>
///     Displays and manages the authenticated user's profile settings.
/// </summary>
public sealed partial class ProfileView : IStateObserver<ProfileState>
{
    private const double EnabledFormOpacity = 1.0;
    private const double DisabledFormOpacity = 0.6;
    private const int FirstGridColumnIndex = 0;
    private const int SecondGridColumnIndex = 1;
    private const int NotificationPreferenceVerticalMargin = 6;
    private const int NotificationPreferenceFontSize = 13;
    private const int SessionCardCornerRadius = 10;
    private const int SessionCardBorderThickness = 1;
    private const int SessionCardHorizontalPadding = 16;
    private const int SessionCardVerticalPadding = 12;
    private const int SessionInfoStackSpacing = 2;
    private const int SessionPrimaryTextFontSize = 13;
    private const int SessionSecondaryTextFontSize = 12;
    private const int SessionMutedTextFontSize = 11;
    private const byte OpaqueColorAlpha = 255;
    private const byte SessionCardBorderRed = 226;
    private const byte SessionCardBorderGreen = 232;
    private const byte SessionCardBorderBlue = 240;
    private const byte SessionPrimaryTextRed = 30;
    private const byte SessionPrimaryTextGreen = 41;
    private const byte SessionPrimaryTextBlue = 59;
    private const byte SessionSecondaryTextRed = 100;
    private const byte SessionSecondaryTextGreen = 116;
    private const byte SessionSecondaryTextBlue = 139;
    private const byte SessionMutedTextRed = 148;
    private const byte SessionMutedTextGreen = 163;
    private const byte SessionMutedTextBlue = 184;
    private readonly IAppNavigationService _navigationService;
    private readonly ProfileViewModel _viewModel;
    private bool _isChangingPasswordFlow;
    private bool _isTwoFactorFlow;
    private bool _isUpdatingToggle;
    private string _pendingTwoFactorAuthType = string.Empty;
    private string _verifiedPassword = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that loads profile data and drives all profile update operations.</param>
    /// <param name="navigationService">Used to navigate to the dashboard or back to login.</param>
    public ProfileView(ProfileViewModel viewModel, IAppNavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _viewModel.State.AddObserver(this);
        Loaded += OnPageLoaded;
    }

    /// <inheritdoc />
    /// <param name="state">The state value.</param>
    public void Update(ProfileState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_isUpdatingToggle)
            {
                if (state == ProfileState.Error)
                {
                    ShowError("Failed to save notification preferences.");
                }

                return;
            }

            switch (state)
            {
                case ProfileState.Loading:
                    ShowLoading(true);
                    break;
                case ProfileState.UpdateSuccess:
                    ShowLoading(false);
                    PopulateUi();
                    break;
                case ProfileState.Error:
                    ShowLoading(false);
                    ShowError("Operation failed.");
                    break;
                case ProfileState.Idle:
                case ProfileState.Success:
                case ProfileState.PasswordChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    /// <inheritdoc />
    /// <param name="e">The e value.</param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    /// <inheritdoc />
    /// <param name="e">The e value.</param>
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _viewModel.State.RemoveObserver(this);
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        ShowLoading(true);
        await _viewModel.LoadProfile();
        ShowLoading(false);
        PopulateUi();
        SetEditingEnabled(false);
    }

    private void PopulateUi()
    {
        ProfileInfo user = _viewModel.ProfileInfo;
        ProfileCardName.Text = user.FullName ?? string.Empty;
        ProfileCardEmail.Text = user.Email ?? string.Empty;
        ProfileCardPhone.Text = user.PhoneNumber ?? string.Empty;
        ProfileCardAddress.Text = user.Address ?? string.Empty;
        FullNameBox.Text = user.FullName ?? string.Empty;
        EmailBox.Text = user.Email ?? string.Empty;
        PhoneBox.Text = user.PhoneNumber ?? string.Empty;
        AddressBox.Text = user.Address ?? string.Empty;
        TwoFactorPhoneDisplay.Text = user.PhoneNumber ?? string.Empty;
        TwoFactorEmailDisplay.Text = user.Email ?? string.Empty;
        _viewModel.IsInitializingView = true;
        TwoFactorToggle.IsOn = user.Is2FaEnabled;
        _viewModel.IsInitializingView = false;
        PopulateOAuthLinks(_viewModel.OAuth.OAuthLinks);
        PopulateNotificationPreferences(_viewModel.Notifications.NotificationPreferences);
        Update2FaVisuals();
    }

    private void SetEditingEnabled(bool enabled)
    {
        FullNameBox.IsEnabled = enabled;
        PhoneBox.IsEnabled = enabled;
        AddressBox.IsEnabled = enabled;
        SaveButton.IsEnabled = enabled;
        FullNameBox.IsReadOnly = !enabled;
        PhoneBox.IsReadOnly = !enabled;
        AddressBox.IsReadOnly = !enabled;
        FullNameBox.Opacity = enabled ? EnabledFormOpacity : DisabledFormOpacity;
        PhoneBox.Opacity = enabled ? EnabledFormOpacity : DisabledFormOpacity;
        AddressBox.Opacity = enabled ? EnabledFormOpacity : DisabledFormOpacity;
        if (!enabled)
        {
            return;
        }

        PhoneBox.Focus(FocusState.Programmatic);
        AddressBox.Focus(FocusState.Programmatic);
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        _isChangingPasswordFlow = false;
        _isTwoFactorFlow = false;
        VerifyCurrentPasswordBox.Password = string.Empty;
        VerifyErrorInfoBar.IsOpen = false;
        await VerifyPasswordDialog.ShowAsync();
    }

    private async void VerifyPasswordDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs arguments)
    {
        ContentDialogButtonClickDeferral? deferral = arguments.GetDeferral();
        if (string.IsNullOrWhiteSpace(VerifyCurrentPasswordBox.Password))
        {
            VerifyErrorInfoBar.Message = "Enter your password.";
            VerifyErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        bool verified = await _viewModel.PersonalInfo.VerifyPassword(VerifyCurrentPasswordBox.Password);
        if (!verified)
        {
            VerifyErrorInfoBar.Message = "Incorrect password.";
            VerifyErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        _verifiedPassword = VerifyCurrentPasswordBox.Password;
        VerifyErrorInfoBar.IsOpen = false;
        deferral.Complete();
        if (_isChangingPasswordFlow)
        {
            DispatcherQueue.TryEnqueue(async void () =>
            {
                NewPasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
                NewPasswordErrorInfoBar.IsOpen = false;
                await NewPasswordDialog.ShowAsync();
            });
        }
        else if (_isTwoFactorFlow)
        {
            DispatcherQueue.TryEnqueue(async void () =>
            {
                await Handle2FaActionAfterVerifyAsync();
            });
        }
        else
        {
            SetEditingEnabled(true);
            ShowSuccess("You can now edit your profile.");
        }
    }

    private async Task Handle2FaActionAfterVerifyAsync()
    {
        TwoFactorMethod method = _pendingTwoFactorAuthType == "Phone"
            ? TwoFactorMethod.Phone
            : TwoFactorMethod.Email;
        bool success = await _viewModel.EnableTwoFactor(method);
        if (success)
        {
            _viewModel.IsInitializingView = true;
            TwoFactorToggle.IsOn = true;
            _viewModel.IsInitializingView = false;
            Update2FaVisuals();
            ShowSuccess($"2FA enabled via {_pendingTwoFactorAuthType}.");
        }
        else
        {
            ShowError($"Failed to enable 2FA via {_pendingTwoFactorAuthType}.");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ShowLoading(true);
        bool success = await _viewModel.PersonalInfo.UpdatePersonalInfo(
            PhoneBox.Text,
            AddressBox.Text,
            _verifiedPassword,
            FullNameBox.Text);
        ShowLoading(false);
        if (success)
        {
            ProfileCardName.Text = FullNameBox.Text.Trim();
            ProfileCardPhone.Text = PhoneBox.Text.Trim();
            ProfileCardAddress.Text = AddressBox.Text.Trim();
            _verifiedPassword = string.Empty;
            SetEditingEnabled(false);
            ShowSuccess("Profile updated successfully.");
        }
        else
        {
            ShowError("Failed to update profile.");
        }
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _isChangingPasswordFlow = true;
        _isTwoFactorFlow = false;
        VerifyCurrentPasswordBox.Password = string.Empty;
        VerifyErrorInfoBar.IsOpen = false;
        await VerifyPasswordDialog.ShowAsync();
    }

    private async void NewPasswordDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs arguments)
    {
        ContentDialogButtonClickDeferral? deferral = arguments.GetDeferral();
        string? newPassword = NewPasswordBox.Password;
        string? confirmPassword = ConfirmPasswordBox.Password;
        int? userId = _viewModel.ProfileInfo.UserId;
        if (userId == null)
        {
            NewPasswordErrorInfoBar.Message = "User not loaded.";
            NewPasswordErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
            return;
        }

        (bool success, string errorMessage) = await _viewModel.Security.ChangePassword(
            userId.Value,
            _verifiedPassword,
            newPassword,
            confirmPassword);
        if (success)
        {
            _verifiedPassword = string.Empty;
            NewPasswordErrorInfoBar.IsOpen = false;
            deferral.Complete();
            ShowSuccess("Your password has been changed successfully.");
        }
        else
        {
            NewPasswordErrorInfoBar.Message = errorMessage;
            NewPasswordErrorInfoBar.IsOpen = true;
            arguments.Cancel = true;
            deferral.Complete();
        }
    }

    private async void Handle2FAAction_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        _pendingTwoFactorAuthType = button?.Tag.ToString() ?? string.Empty;
        if (button?.Content.ToString() == "Remove")
        {
            bool success = await _viewModel.DisableTwoFactor();
            if (success)
            {
                _viewModel.IsInitializingView = true;
                TwoFactorToggle.IsOn = false;
                _viewModel.IsInitializingView = false;
                Update2FaVisuals();
                ShowSuccess("2FA has been disabled.");
            }
            else
            {
                ShowError("Failed to remove 2FA.");
            }
        }
        else
        {
            _isTwoFactorFlow = true;
            _isChangingPasswordFlow = false;
            VerifyCurrentPasswordBox.Password = string.Empty;
            VerifyErrorInfoBar.IsOpen = false;
            await VerifyPasswordDialog.ShowAsync();
        }
    }

    private async void TwoFactorToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsInitializingView)
        {
            return;
        }

        bool success = await _viewModel.SetEmailTwoFactorEnabled(TwoFactorToggle.IsOn);
        if (!success)
        {
            _viewModel.IsInitializingView = true;
            TwoFactorToggle.IsOn = !TwoFactorToggle.IsOn;
            _viewModel.IsInitializingView = false;
            ShowError("Failed to update 2FA settings.");
        }
        else
        {
            Update2FaVisuals();
        }
    }

    private async void RemoveConnectedAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: OAuthLinkDataTransferObject link })
        {
            bool success = await _viewModel.OAuth.UnlinkOAuth(link.Provider);
            if (success)
            {
                PopulateOAuthLinks(_viewModel.OAuth.OAuthLinks);
            }
            else
            {
                ShowError("Failed to remove account.");
            }
        }
    }

    private void ManageDevicesButton_Click(object sender, RoutedEventArgs e)
    {
    }

    private async void NotificationToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsInitializingView)
        {
            return;
        }

        if (sender is ToggleSwitch { Tag: NotificationPreferenceDataTransferObject preference } toggle)
        {
            _isUpdatingToggle = true;
            await _viewModel.ToggleNotificationPreference(preference, toggle.IsOn);
            _isUpdatingToggle = false;
            _viewModel.IsInitializingView = true;
            toggle.IsOn = preference.EmailEnabled;
            _viewModel.IsInitializingView = false;
        }
    }

    private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<DashboardView>();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<LoginView>();
    }

    private void Update2FaVisuals()
    {
        TwoFactorPhoneDisplay.Text = _viewModel.PersonalInfo.TwoFactorPhoneDisplay;
        if (!_viewModel.PersonalInfo.HasPhoneNumber)
        {
            ConfigureActionButton(
                ActionPhoneBtn,
                PhoneStatusBadge,
                PhoneStatusText,
                "Add",
                "#F1F5F9",
                "#64748B",
                "Not configured");
        }
        else if (_viewModel.IsPhoneTwoFactorActive)
        {
            ConfigureActionButton(
                ActionPhoneBtn,
                PhoneStatusBadge,
                PhoneStatusText,
                "Remove",
                "#DCFCE7",
                "#16A34A",
                "Active");
        }
        else
        {
            ConfigureActionButton(
                ActionPhoneBtn,
                PhoneStatusBadge,
                PhoneStatusText,
                "Verify",
                "#FFF7ED",
                "#C2410C",
                "Unverified");
        }

        if (_viewModel.IsEmailTwoFactorActive)
        {
            ConfigureActionButton(
                ActionEmailBtn,
                EmailStatusBadge,
                EmailStatusText,
                "Remove",
                "#DCFCE7",
                "#16A34A",
                "Active");
        }
        else
        {
            ConfigureActionButton(
                ActionEmailBtn,
                EmailStatusBadge,
                EmailStatusText,
                "Verify",
                "#FFF7ED",
                "#C2410C",
                "Unverified");
        }
    }

    private void ConfigureActionButton(
        Button button,
        Border badge,
        TextBlock statusText,
        string action,
        string badgeBg,
        string textCol,
        string status)
    {
        button.Content = action;
        statusText.Text = status;
    }

    private void ShowLoading(bool visible)
    {
        LoadingPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        LoadingRing.IsActive = visible;
        ErrorInfoBar.IsOpen = false;
        SuccessInfoBar.IsOpen = false;
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
        SuccessInfoBar.IsOpen = false;
    }

    private void ShowSuccess(string message)
    {
        SuccessInfoBar.Message = message;
        SuccessInfoBar.IsOpen = true;
        ErrorInfoBar.IsOpen = false;
    }

    private void TabPersonalBtn_Click(object sender, RoutedEventArgs e)
    {
        PanelPersonal.Visibility = Visibility.Visible;
        PanelSecurity.Visibility = Visibility.Collapsed;
        PanelNotifications.Visibility = Visibility.Collapsed;
        PanelSessions.Visibility = Visibility.Collapsed;
        TabPersonalBtn.Style = (Style)Resources["TabButtonActiveStyle"];
        TabSecurityBtn.Style = (Style)Resources["TabButtonStyle"];
        TabNotificationsBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSessionsBtn.Style = (Style)Resources["TabButtonStyle"];
    }

    private void TabSecurityBtn_Click(object sender, RoutedEventArgs e)
    {
        PanelPersonal.Visibility = Visibility.Collapsed;
        PanelSecurity.Visibility = Visibility.Visible;
        PanelNotifications.Visibility = Visibility.Collapsed;
        PanelSessions.Visibility = Visibility.Collapsed;
        TabPersonalBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSecurityBtn.Style = (Style)Resources["TabButtonActiveStyle"];
        TabNotificationsBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSessionsBtn.Style = (Style)Resources["TabButtonStyle"];
    }

    private void TabNotificationsBtn_Click(object sender, RoutedEventArgs e)
    {
        PanelPersonal.Visibility = Visibility.Collapsed;
        PanelSecurity.Visibility = Visibility.Collapsed;
        PanelNotifications.Visibility = Visibility.Visible;
        PanelSessions.Visibility = Visibility.Collapsed;
        TabPersonalBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSecurityBtn.Style = (Style)Resources["TabButtonStyle"];
        TabNotificationsBtn.Style = (Style)Resources["TabButtonActiveStyle"];
        TabSessionsBtn.Style = (Style)Resources["TabButtonStyle"];
    }

    private void PopulateOAuthLinks(List<OAuthLinkDataTransferObject>? links)
    {
        OAuthLinksPanel.Children.Clear();
        if (links == null)
        {
            return;
        }

        foreach (Button button in links.Select(link => new Button
                 {
                     Content = link.ProviderEmail ?? link.Provider,
                     Tag = link,
                 }))
        {
            button.Click += RemoveConnectedAccount_Click;
            OAuthLinksPanel.Children.Add(button);
        }
    }

    private void PopulateNotificationPreferences(List<NotificationPreferenceDataTransferObject>? preferences)
    {
        _viewModel.IsInitializingView = true;
        NotificationPreferencesPanel.Children.Clear();
        if (preferences == null)
        {
            _viewModel.IsInitializingView = false;
            return;
        }

        foreach (NotificationPreferenceDataTransferObject preference in preferences)
        {
            var row = new Grid
            {
                Margin = new Thickness(
                    0,
                    NotificationPreferenceVerticalMargin,
                    0,
                    NotificationPreferenceVerticalMargin),
            };
            row.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(SecondGridColumnIndex, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var text = new TextBlock
            {
                Text = preference.Category.ToDisplayName(),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = NotificationPreferenceFontSize,
                Foreground = (Brush)Resources["TextPrimary"],
            };
            var toggle = new ToggleSwitch
            {
                IsOn = preference.EmailEnabled,
                Tag = preference,
                VerticalAlignment = VerticalAlignment.Center,
            };
            toggle.Toggled += NotificationToggle_Toggled;
            Grid.SetColumn(text, FirstGridColumnIndex);
            Grid.SetColumn(toggle, SecondGridColumnIndex);
            row.Children.Add(text);
            row.Children.Add(toggle);
            NotificationPreferencesPanel.Children.Add(row);
        }

        _viewModel.IsInitializingView = false;
    }

    private async void TabSessionsBtn_Click(object sender, RoutedEventArgs e)
    {
        PanelPersonal.Visibility = Visibility.Collapsed;
        PanelSecurity.Visibility = Visibility.Collapsed;
        PanelNotifications.Visibility = Visibility.Collapsed;
        PanelSessions.Visibility = Visibility.Visible;
        TabPersonalBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSecurityBtn.Style = (Style)Resources["TabButtonStyle"];
        TabNotificationsBtn.Style = (Style)Resources["TabButtonStyle"];
        TabSessionsBtn.Style = (Style)Resources["TabButtonActiveStyle"];
        await LoadSessionsAsync();
    }

    private async Task<bool> LoadSessionsAsync()
    {
        SessionsErrorBar.IsOpen = false;
        SessionsSuccessBar.IsOpen = false;
        SessionsListPanel.Children.Clear();
        NoSessionsText.Visibility = Visibility.Collapsed;
        (bool loaded, string? errorMessage) = await _viewModel.LoadSessionsForCurrentUser();
        if (!loaded)
        {
            SessionsErrorBar.Message = errorMessage ?? "Failed to load active sessions.";
            SessionsErrorBar.IsOpen = true;
            return false;
        }

        RenderSessions();
        return true;
    }

    private Border BuildSessionCard(SessionDataTransferObject session)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            CornerRadius = new CornerRadius(SessionCardCornerRadius),
            BorderBrush = new SolidColorBrush(
                ColorHelper.FromArgb(
                    OpaqueColorAlpha,
                    SessionCardBorderRed,
                    SessionCardBorderGreen,
                    SessionCardBorderBlue)),
            BorderThickness = new Thickness(SessionCardBorderThickness),
            Padding = new Thickness(
                SessionCardHorizontalPadding,
                SessionCardVerticalPadding,
                SessionCardHorizontalPadding,
                SessionCardVerticalPadding),
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(SecondGridColumnIndex, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var infoStack = new StackPanel { Spacing = SessionInfoStackSpacing };
        var deviceText = new TextBlock
        {
            Text = session.DeviceInfo ?? "Unknown Device",
            FontSize = SessionPrimaryTextFontSize,
            Foreground = new SolidColorBrush(
                ColorHelper.FromArgb(
                    OpaqueColorAlpha,
                    SessionPrimaryTextRed,
                    SessionPrimaryTextGreen,
                    SessionPrimaryTextBlue)),
        };
        var browserText = new TextBlock
        {
            Text = session.Browser ?? "Unknown Browser",
            FontSize = SessionSecondaryTextFontSize,
            Foreground = new SolidColorBrush(
                ColorHelper.FromArgb(
                    OpaqueColorAlpha,
                    SessionSecondaryTextRed,
                    SessionSecondaryTextGreen,
                    SessionSecondaryTextBlue)),
        };
        var networkAddressText = new TextBlock
        {
            Text = $"IP: {session.IpAddress ?? "Unknown"}",
            FontSize = SessionSecondaryTextFontSize,
            Foreground = new SolidColorBrush(
                ColorHelper.FromArgb(
                    OpaqueColorAlpha,
                    SessionSecondaryTextRed,
                    SessionSecondaryTextGreen,
                    SessionSecondaryTextBlue)),
        };
        var lastActiveText = new TextBlock
        {
            Text = session.LastActiveAt.HasValue
                ? $"Last active: {session.LastActiveAt.Value:g}"
                : "Last active: Unknown",
            FontSize = SessionMutedTextFontSize,
            Foreground = new SolidColorBrush(
                ColorHelper.FromArgb(
                    OpaqueColorAlpha,
                    SessionMutedTextRed,
                    SessionMutedTextGreen,
                    SessionMutedTextBlue)),
        };
        infoStack.Children.Add(deviceText);
        infoStack.Children.Add(browserText);
        infoStack.Children.Add(networkAddressText);
        infoStack.Children.Add(lastActiveText);
        var revokeButton = new Button
        {
            Content = "Revoke",
            Tag = session.Id,
            VerticalAlignment = VerticalAlignment.Center,
            Style = (Style)Resources["DangerButtonStyle"],
        };
        revokeButton.Click += RevokeSessionButton_Click;
        Grid.SetColumn(infoStack, FirstGridColumnIndex);
        Grid.SetColumn(revokeButton, SecondGridColumnIndex);
        grid.Children.Add(infoStack);
        grid.Children.Add(revokeButton);
        card.Child = grid;
        return card;
    }

    private async void RevokeSessionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int sessionId })
        {
            return;
        }

        (bool success, string? errorMessage) = await _viewModel.RevokeSessionAndReload(sessionId);
        if (success)
        {
            RenderSessions();
            SessionsSuccessBar.Message = "Session revoked successfully.";
            SessionsSuccessBar.IsOpen = true;
        }
        else
        {
            SessionsErrorBar.Message = errorMessage ?? "Failed to revoke session.";
            SessionsErrorBar.IsOpen = true;
        }
    }

    private void RenderSessions()
    {
        SessionsListPanel.Children.Clear();
        NoSessionsText.Visibility = Visibility.Collapsed;
        if (_viewModel.Sessions.ActiveSessions.Count == default)
        {
            NoSessionsText.Visibility = Visibility.Visible;
            return;
        }

        foreach (SessionDataTransferObject session in _viewModel.Sessions.ActiveSessions)
        {
            SessionsListPanel.Children.Add(BuildSessionCard(session));
        }
    }
}
