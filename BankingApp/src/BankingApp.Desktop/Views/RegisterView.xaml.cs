// <copyright file="RegisterView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for RegisterView.xaml.
// </summary>

using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Master;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Displays the registration form and reacts to registration state changes.
/// </summary>
public sealed partial class RegisterView : IStateObserver<RegisterState>
{
    private readonly IAppNavigationService _navigationService;
    private readonly IRegistrationContext _registrationContext;
    private readonly RegisterViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegisterView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives registration logic and exposes registration state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="registrationContext">Carries the just-registered flag to the login page.</param>
    /// <returns>The result of the operation.</returns>
    public RegisterView(
        RegisterViewModel viewModel,
        IAppNavigationService navigationService,
        IRegistrationContext registrationContext)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigationService = navigationService;
        _registrationContext = registrationContext;
        _viewModel.State.AddObserver(this);
    }

    /// <inheritdoc />
    /// <param name="state">The state value.</param>
    public void Update(RegisterState state)
    {
        OnStateChanged(state);
    }

    private void OnStateChanged(RegisterState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            HideLoading();
            ErrorInfoBar.IsOpen = false;
            switch (state)
            {
                case RegisterState.Loading:
                    ShowLoading();
                    break;
                case RegisterState.Success:
                    _registrationContext.JustRegistered = true;
                    _navigationService.NavigateTo<LoginView>();
                    break;
                case RegisterState.AutoLoggedIn:
                    _navigationService.NavigateTo<NavView>();
                    break;
                case RegisterState.EmailAlreadyExists:
                    ShowError(UserMessages.Register.EmailAlreadyExists);
                    break;
                case RegisterState.InvalidEmail:
                    ShowError(UserMessages.Register.InvalidEmail);
                    break;
                case RegisterState.WeakPassword:
                    ShowError(UserMessages.Register.WeakPassword);
                    break;
                case RegisterState.PasswordMismatch:
                    ShowError(UserMessages.Register.PasswordMismatch);
                    break;
                case RegisterState.Error:
                    ShowError(UserMessages.Register.AllFieldsRequired);
                    break;
            }
        });
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }

    private void ShowLoading()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
        RegisterButton.IsEnabled = false;
    }

    private void HideLoading()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
        RegisterButton.IsEnabled = true;
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.Register(
            EmailBox.Text,
            PasswordBox.Password,
            ConfirmPasswordBox.Password,
            FullNameBox.Text);
    }

    private async void GoogleRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.OAuthRegister("Google");
    }

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<LoginView>();
    }
}