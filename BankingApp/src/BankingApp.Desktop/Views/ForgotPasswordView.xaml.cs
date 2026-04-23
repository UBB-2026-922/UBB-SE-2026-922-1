// <copyright file="ForgotPasswordView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for ForgotPasswordView.xaml.
// </summary>

using System;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Master;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Displays the account recovery flow for requesting a reset code and setting a new password.
///     This code-behind contains only UI-specific logic (loading state, message display, navigation).
///     All business validation and state transitions are handled by <see cref="ForgotPasswordViewModel" />.
/// </summary>
public sealed partial class ForgotPasswordView : IStateObserver<ForgotPasswordState>
{
    private readonly IAppNavigationService _navigationService;
    private readonly ForgotPasswordViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForgotPasswordView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives password recovery logic and exposes recovery state.</param>
    /// <param name="navigationService">Used to navigate back to the login page after recovery completes.</param>
    /// <returns>The result of the operation.</returns>
    public ForgotPasswordView(ForgotPasswordViewModel viewModel, IAppNavigationService navigationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _viewModel.State.AddObserver(this);
    }

    /// <inheritdoc />
    /// <param name="state">The state value.</param>
    public void Update(ForgotPasswordState state)
    {
        OnStateChanged(state);
    }

    private void OnStateChanged(ForgotPasswordState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            HideLoading();
            if (state == ForgotPasswordState.Error && !string.IsNullOrWhiteSpace(_viewModel.ValidationError))
            {
                ShowMessage(_viewModel.ValidationError, InfoBarSeverity.Warning);
                return;
            }

            switch (state)
            {
                case ForgotPasswordState.Idle:
                    Step1Panel.Visibility = Visibility.Visible;
                    Step2Panel.Visibility = Visibility.Collapsed;
                    break;
                case ForgotPasswordState.EmailSent:
                    ShowMessage("A recovery code has been sent to your email.", InfoBarSeverity.Success);
                    InstructionText.Text = "Please paste the code from your email to continue.";
                    Step1Panel.Visibility = Visibility.Collapsed;
                    Step2Panel.Visibility = Visibility.Visible;
                    Step3Panel.Visibility = Visibility.Collapsed;
                    VerifyTokenButton.Visibility = Visibility.Visible;
                    ResendPanel.Visibility = Visibility.Visible;
                    TokenBox.IsEnabled = true;
                    TokenBox.Text = string.Empty;
                    break;
                case ForgotPasswordState.PasswordResetSuccess:
                    ShowMessage(
                        "Your password has been reset successfully! You can now log in.",
                        InfoBarSeverity.Success);
                    Step1Panel.Visibility = Visibility.Collapsed;
                    Step2Panel.Visibility = Visibility.Collapsed;
                    InstructionText.Text = "Account recovered successfully.";
                    break;
                case ForgotPasswordState.TokenValid:
                    ShowMessage("Code verified! You can now set a new password.", InfoBarSeverity.Success);
                    VerifyTokenButton.Visibility = Visibility.Collapsed;
                    ResendPanel.Visibility = Visibility.Collapsed;
                    TokenBox.IsEnabled = false;
                    Step3Panel.Visibility = Visibility.Visible;
                    break;
                case ForgotPasswordState.TokenExpired:
                    ShowMessage("The recovery code has expired. Please request a new one.", InfoBarSeverity.Error);
                    break;
                case ForgotPasswordState.TokenAlreadyUsed:
                    ShowMessage("This recovery code has already been used.", InfoBarSeverity.Error);
                    break;
                case ForgotPasswordState.Error:
                    ShowMessage("An error occurred. Please try again.", InfoBarSeverity.Error);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
    {
        StatusInfoBar.IsOpen = false;
        ShowLoading();
        await _viewModel.ForgotPassword(EmailBox.Text.Trim());
    }

    private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        StatusInfoBar.IsOpen = false;
        if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
        {
            ShowMessage("Passwords do not match.", InfoBarSeverity.Warning);
            return;
        }

        ShowLoading();
        await _viewModel.ResetPassword(NewPasswordBox.Password, TokenBox.Text.Trim());
    }

    private async void VerifyTokenButton_Click(object sender, RoutedEventArgs e)
    {
        StatusInfoBar.IsOpen = false;
        ShowLoading();
        await _viewModel.VerifyToken(TokenBox.Text.Trim());
    }

    private async void ResendCodeButton_Click(object sender, RoutedEventArgs e)
    {
        StatusInfoBar.IsOpen = false;
        ShowLoading();
        await _viewModel.ForgotPassword(EmailBox.Text.Trim());
    }

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<LoginView>();
    }

    /// <summary>
    ///     Shows a status message in the view's info bar.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="severity">The severity level.</param>
    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;
        StatusInfoBar.IsOpen = true;
    }

    private void ShowLoading()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
        SendCodeButton.IsEnabled = false;
        ResetPasswordButton.IsEnabled = false;
    }

    private void HideLoading()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
        SendCodeButton.IsEnabled = true;
        ResetPasswordButton.IsEnabled = true;
    }
}