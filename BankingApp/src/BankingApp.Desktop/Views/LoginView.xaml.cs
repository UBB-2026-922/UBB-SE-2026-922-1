// <copyright file="LoginView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for LoginView.xaml.
// </summary>

using System;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Master;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Displays the login form and reacts to authentication state changes produced by <see cref="LoginViewModel" />.
/// </summary>
public sealed partial class LoginView : IStateObserver<LoginState>
{
    private readonly IAppNavigationService _navigationService;
    private readonly LoginViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives authentication logic and exposes login state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="registrationContext">Carries the just-registered flag set by the register page.</param>
    /// <returns>The result of the operation.</returns>
    public LoginView(
        LoginViewModel viewModel,
        IAppNavigationService navigationService,
        IRegistrationContext registrationContext)
    {
        _navigationService = navigationService;
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.State.AddObserver(this);
        if (registrationContext.JustRegistered)
        {
            registrationContext.JustRegistered = false;
            RegistrationSuccessBar.IsOpen = true;
        }

        // Apply the ViewModel's current state immediately. The ViewModel is constructed
        // before the view subscribes, so any state set in the constructor (e.g.
        // ServerNotConfigured when ApiBaseUrl is missing) would otherwise be missed.
        OnStateChanged(_viewModel.State.Value);
    }

    /// <inheritdoc />
    /// <param name="state">The state value.</param>
    public void Update(LoginState state)
    {
        OnStateChanged(state);
    }

    private void OnStateChanged(LoginState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            HideLoading();
            ErrorInfoBar.IsOpen = false;
            switch (state)
            {
                case LoginState.Idle:
                    EnableForm();
                    break;
                case LoginState.Loading:
                    ShowLoading();
                    break;
                case LoginState.Success:
                    EnableForm();
                    _navigationService.NavigateTo<NavView>();
                    break;
                case LoginState.Require2Fa:
                    EnableForm();
                    _navigationService.NavigateTo<TwoFactorView>();
                    break;
                case LoginState.InvalidCredentials:
                    EnableForm();
                    ShowError("Invalid email or password.");
                    break;
                case LoginState.AccountLocked:
                    EnableForm();
                    ShowError("Account is locked. Try again later.");
                    break;
                case LoginState.Error:
                    EnableForm();
                    ShowError("Something went wrong. Please try again.");
                    break;
                case LoginState.ServerNotConfigured:
                    // Form stays disabled — misconfiguration cannot be resolved at runtime.
                    ShowError("The application is not properly set up. Please contact your administrator.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }

    private void EnableForm()
    {
        SignInButton.IsEnabled = true;
        GoogleLoginButton.IsEnabled = true;
    }

    private void ShowLoading()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
        SignInButton.IsEnabled = false;
        GoogleLoginButton.IsEnabled = false;
    }

    private void HideLoading()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
    }

    private async void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        string? email = EmailBox.Text;
        string? password = PasswordBox.Password;
        if (!_viewModel.CanLogin(email, password))
        {
            ShowError("Please enter email and password.");
            return;
        }

        await _viewModel.Login(email, password);
    }

    private async void GoogleLoginButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.OAuthLogin("Google");
    }

    private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<ForgotPasswordView>();
    }

    private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<RegisterView>();
    }
}