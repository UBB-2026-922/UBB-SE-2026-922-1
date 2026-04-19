// <copyright file="DashboardView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for DashboardView.xaml.
// </summary>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace BankApp.Desktop.Views;

/// <summary>
///     Displays the authenticated user's account summary, card carousel, and recent transactions.
/// </summary>
public sealed partial class DashboardView : IStateObserver<DashboardState>
{
    private const int ActiveCardDotSize = 18;
    private const int InactiveCardDotSize = 8;
    private const int MinimumVisibleDotCount = 1;
    private const int FirstCardDotIndex = 0;
    private const byte ActiveDotAlpha = 255;
    private const byte InactiveDotAlpha = 100;
    private const byte DotRedChannel = 78;
    private const byte DotGreenChannel = 205;
    private const byte DotBlueChannel = 196;

    private static readonly Color _activeDotColor = Color.FromArgb(
        ActiveDotAlpha,
        DotRedChannel,
        DotGreenChannel,
        DotBlueChannel);

    private static readonly Color _inactiveDotColor = Color.FromArgb(
        InactiveDotAlpha,
        DotRedChannel,
        DotGreenChannel,
        DotBlueChannel);

    private readonly DashboardViewModel _viewModel;
    private bool _isObserverAttached;
    private CancellationTokenSource? _loadCancellationTokenSource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DashboardView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that loads account data and exposes dashboard state.</param>
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        Loaded += OnPageLoaded;
    }

    /// <inheritdoc />
    /// <param name="state">The state value.</param>
    public void Update(DashboardState state)
    {
        OnStateChanged(state);
    }

    /// <inheritdoc />
    /// <param name="e">The e value.</param>
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        CancelPendingLoad();
        DetachObserver();
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        AttachObserver();
        _ = RunUiTaskAsync(LoadDashboardAsync);
    }

    /// <summary>
    ///     Reacts to dashboard state updates from the view model.
    /// </summary>
    /// <param name="state">The new state.</param>
    private void OnStateChanged(DashboardState state)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (state)
            {
                case DashboardState.Loading:
                    ShowLoading();
                    break;
                case DashboardState.Success:
                    HideLoading();
                    ErrorInfoBar.IsOpen = false;
                    RefreshUi();
                    break;
                case DashboardState.Error:
                    HideLoading();
                    ShowError(_viewModel.ErrorMessage);
                    break;
                case DashboardState.Idle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private async Task LoadDashboardAsync()
    {
        CancelPendingLoad();
        _loadCancellationTokenSource = new CancellationTokenSource();
        try
        {
            await _viewModel.LoadDashboard(_loadCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void RefreshUi()
    {
        UserNameText.Text = _viewModel.CurrentUser?.FullName ?? string.Empty;
        TransactionsList.ItemsSource = _viewModel.RecentTransactionItems;
        // Visibility decided by ViewModel state; View only does the mapping to Visibility enum.
        EmptyTransactionsState.Visibility =
            _viewModel.HasTransactions ? Visibility.Collapsed : Visibility.Visible;
        BuildCardDots();
        ShowCard();
        NavView.Current?.UpdateNotificationBadge(_viewModel.UnreadNotificationCount);
    }

    /// <summary>
    ///     Renders the card at the current index stored in the ViewModel.
    ///     All data and formatting decisions come from the ViewModel.
    /// </summary>
    private void ShowCard()
    {
        if (!_viewModel.HasCards)
        {
            CardVisual.Visibility = Visibility.Collapsed;
            EmptyCardsState.Visibility = Visibility.Visible;
            ClearCardDisplay();
            UpdateCardNavigationState();
            return;
        }

        CardVisual.Visibility = Visibility.Visible;
        EmptyCardsState.Visibility = Visibility.Collapsed;
        // All formatting decisions are delegated to the ViewModel.
        CardBankName.Text = "BankApp";
        CardBrandName.Text = _viewModel.SelectedCardBrandDisplay;
        CardHolderText.Text = _viewModel.SelectedCardHolderDisplay;
        CardExpiryText.Text = _viewModel.SelectedCardExpiryDisplay;
        CardNumberText.Text = _viewModel.SelectedCardNumberMasked;
        UpdateCardDots();
        UpdateCardNavigationState();
    }

    private void BuildCardDots()
    {
        CardDots.Children.Clear();
        IReadOnlyList<CardPageIndicatorViewModel> dots = _viewModel.CardDots;
        CardDots.Visibility = dots.Count > MinimumVisibleDotCount ? Visibility.Visible : Visibility.Collapsed;
        foreach (CardPageIndicatorViewModel dotViewModel in dots)
        {
            var dot = new Ellipse
            {
                Width = dotViewModel.IsActive ? ActiveCardDotSize : InactiveCardDotSize,
                Height = InactiveCardDotSize,
                Fill = new SolidColorBrush(dotViewModel.IsActive ? _activeDotColor : _inactiveDotColor),
            };
            CardDots.Children.Add(dot);
        }
    }

    private void UpdateCardDots()
    {
        IReadOnlyList<CardPageIndicatorViewModel> dots = _viewModel.CardDots;
        for (int index = FirstCardDotIndex; index < CardDots.Children.Count; index++)
        {
            if (CardDots.Children[index] is not Ellipse dot || index >= dots.Count)
            {
                continue;
            }

            dot.Width = dots[index].IsActive ? ActiveCardDotSize : InactiveCardDotSize;
            dot.Fill = new SolidColorBrush(dots[index].IsActive ? _activeDotColor : _inactiveDotColor);
        }
    }

    private void UpdateCardNavigationState()
    {
        // ViewModel decides whether navigation is possible; View maps booleans to UI properties.
        PrevCardButton.IsEnabled = _viewModel.CanNavigatePrevious;
        NextCardButton.IsEnabled = _viewModel.CanNavigateNext;
        PrevCardButton.Visibility = _viewModel.HasCards ? Visibility.Visible : Visibility.Collapsed;
        NextCardButton.Visibility = _viewModel.HasCards ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ClearCardDisplay()
    {
        CardBankName.Text = string.Empty;
        CardBrandName.Text = string.Empty;
        CardHolderText.Text = string.Empty;
        CardExpiryText.Text = string.Empty;
        CardNumberText.Text = "**** **** **** ****";
    }

    private void PrevCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.NavigatePrevious().IsError)
        {
            ShowCard();
        }
    }

    private void NextCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.NavigateNext().IsError)
        {
            ShowCard();
        }
    }

    private void TransferButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RunUiTaskAsync(() => ShowComingSoonAsync("Transfers"));
    }

    private void PayBillButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RunUiTaskAsync(() => ShowComingSoonAsync("Bill Payments"));
    }

    private void ExchangeButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RunUiTaskAsync(() => ShowComingSoonAsync("Currency Exchange"));
    }

    private void TransactionHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RunUiTaskAsync(() => ShowComingSoonAsync("Transaction History"));
    }

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        _ = RunUiTaskAsync(LoadDashboardAsync);
    }

    private void CardVisual_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _ = RunUiTaskAsync(ShowCurrentCardDetailsAsync);
    }

    /// <summary>
    ///     Shows a ContentDialog with the details of the currently selected card.
    ///     The detail string is produced by the ViewModel; this method only handles the dialog UI.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    private async Task ShowCurrentCardDetailsAsync()
    {
        string details = _viewModel.GetSelectedCardDetails();
        if (string.IsNullOrEmpty(details))
        {
            return;
        }

        await ShowAlertAsync("Card Details", details);
    }

    private async Task ShowAlertAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private async Task ShowComingSoonAsync(string feature)
    {
        if (NavView.Current != null)
        {
            await NavView.Current.ShowComingSoonAsync(feature);
            return;
        }

        await ShowAlertAsync(feature, $"{feature} is coming soon.");
    }

    private void ShowLoading()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
    }

    private void HideLoading()
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = string.IsNullOrWhiteSpace(message)
            ? "We couldn't load your dashboard right now."
            : message;
        ErrorInfoBar.IsOpen = true;
    }

    private void AttachObserver()
    {
        if (_isObserverAttached)
        {
            return;
        }

        _viewModel.State.AddObserver(this);
        _isObserverAttached = true;
    }

    private void DetachObserver()
    {
        if (!_isObserverAttached)
        {
            return;
        }

        _viewModel.State.RemoveObserver(this);
        _isObserverAttached = false;
    }

    private void CancelPendingLoad()
    {
        if (_loadCancellationTokenSource is null)
        {
            return;
        }

        _loadCancellationTokenSource.Cancel();
        _loadCancellationTokenSource.Dispose();
        _loadCancellationTokenSource = null;
    }

    private async Task RunUiTaskAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            HideLoading();
            ShowError($"Unexpected error: {exception.Message}");
        }
    }
}