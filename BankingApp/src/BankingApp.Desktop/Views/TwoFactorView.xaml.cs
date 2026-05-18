namespace BankingApp.Desktop.Views;

using System;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Navigation;

/// <summary>
///     Displays the OTP verification step of the login flow.
///     <see cref="TwoFactorViewModel" />.
/// </summary>
public sealed partial class TwoFactorView
{
    private readonly IAuthService _authService;
    private readonly IAppNavigationService _navigationService;

    /// <summary>Initializes a new instance of the <see cref="TwoFactorView"/> class.</summary>
    public TwoFactorView(TwoFactorViewModel viewModel, IAppNavigationService navigationService, IAuthService authService)
    {
        InitializeComponent();
        ViewModel = viewModel;
        _navigationService = navigationService;
        _authService = authService;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        OnStateChanged(ViewModel.State);
    }

    /// <summary>Gets the view model backing the page.</summary>
    public TwoFactorViewModel ViewModel { get; }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TwoFactorViewModel.State))
        {
            DispatcherQueue.TryEnqueue(() => OnStateChanged(ViewModel.State));
        }
    }

    private Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    private void OnStateChanged(TwoFactorState state)
    {
        switch (state)
        {
            case TwoFactorState.Success:
                _navigationService.NavigateTo<NavigationView>();
                break;
            case TwoFactorState.Idle:
            case TwoFactorState.Verifying:
            case TwoFactorState.InvalidOtp:
            case TwoFactorState.Expired:
            case TwoFactorState.MaxAttemptsReached:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async void VerifyButton_Click(object sender, RoutedEventArgs e) => await ViewModel.VerifyOtp();

    private async void ResendButton_Click(object sender, RoutedEventArgs e) => await ViewModel.ResendOtp();

    private void OtpBox_TextChanged(object sender, TextChangedEventArgs e) => ViewModel.OtpCode = OtpBox.Text;

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        _authService.ClearToken();
        _navigationService.NavigateTo<LoginView>();
    }
}
