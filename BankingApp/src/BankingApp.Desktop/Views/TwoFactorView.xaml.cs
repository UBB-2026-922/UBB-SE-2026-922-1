namespace BankingApp.Desktop.Views;

using System;
using Enums;
using BankingApp.Application.Common.Utilities;
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
    private readonly IApiClient _apiClient;
    private readonly IAppNavigationService _navigationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TwoFactorView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives OTP verification logic and exposes two-factor state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="apiClient">Used to clear authentication state when the user cancels and returns to login.</param>
    /// <returns>The result of the operation.</returns>
    public TwoFactorView(TwoFactorViewModel viewModel, IAppNavigationService navigationService, IApiClient apiClient)
    {
        InitializeComponent();
        ViewModel = viewModel;
        _navigationService = navigationService;
        _apiClient = apiClient;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        OnStateChanged(ViewModel.State);
    }

    /// <summary>
    ///     Gets the view model. Exposed as a public property so that compiled
    ///     <c>{x:Bind ViewModel.Property}</c> expressions in the XAML can resolve it.
    /// </summary>
    /// <value>
    ///     The view model. Exposed as a public property so that compiled
    ///     <c>{x:Bind ViewModel.Property}</c> expressions in the XAML can resolve it.
    /// </value>
    public TwoFactorViewModel ViewModel { get; }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TwoFactorViewModel.State))
        {
            DispatcherQueue.TryEnqueue(() => OnStateChanged(ViewModel.State));
        }
    }

    // Used in XAML as: Visibility="{x:Bind BoolToVisibility(ViewModel.SomeBool), Mode=OneWay}"
    // Keeps WinUI-specific Visibility type out of the ViewModel.
    private Visibility BoolToVisibility(bool value)
    {
        return value ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Reacts to state transitions from the ViewModel.
    ///     Most visual state is handled automatically through {x:Bind} — this method
    ///     only handles cases that require imperative navigation calls.
    /// </summary>
    /// <param name="state">The state value.</param>
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
                // Handled through bindings: HasError, ErrorMessage, IsInputEnabled.
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        // Validation (6-digit length check) is enforced inside the ViewModel.
        await ViewModel.VerifyOtp();
    }

    private async void ResendButton_Click(object sender, RoutedEventArgs e)
    {
        // Guard against premature resend is enforced inside the ViewModel.
        await ViewModel.ResendOtp();
    }

    /// <summary>
    ///     Propagates the typed text to the ViewModel so that <see cref="TwoFactorViewModel.OtpCode" />
    ///     is always in sync without requiring a Two-Way binding.
    /// </summary>
    /// <param name="sender">The sender value.</param>
    /// <param name="e">The e value.</param>
    private void OtpBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.OtpCode = OtpBox.Text;
    }

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        _apiClient.ClearToken();
        _navigationService.NavigateTo<LoginView>();
    }
}
