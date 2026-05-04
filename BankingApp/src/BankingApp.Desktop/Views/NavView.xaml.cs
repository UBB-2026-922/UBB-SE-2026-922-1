namespace BankingApp.Desktop.Views;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Master;
using Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

/// <summary>
///     Hosts the application shell after login: renders the sidebar and manages the inner content frame
///     where feature pages (Dashboard, Profile, etc.) are displayed.
/// </summary>
public sealed partial class NavView
{
    private const int MaximumInlineNotificationBadgeCount = 99;
    private const string OverflowNotificationBadgeText = "99+";
    private readonly IApiClient _apiClient;
    private readonly List<Button> _navButtons;
    private readonly IAppNavigationService _navigationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavView" /> class.
    /// </summary>
    /// <param name="apiClient">Used to clear authentication state when the user logs out.</param>
    /// <param name="navigationService">Bound to the inner content frame to drive feature-page navigation.</param>
    public NavView(IApiClient apiClient, IAppNavigationService navigationService)
    {
        InitializeComponent();
        Current = this;
        _navButtons =
        [
            NavDashboard, NavTransfers, NavBillPayments, NavRecurringPayments, NavCards,
            NavTransferHistory, NavCurrencyExchange, NavRateAlerts, NavSavings,
            NavInvestments, NavStatistics, NavSupport, NavProfile,
            NavBeneficiaries
        ];
        _apiClient = apiClient;
        _navigationService = navigationService;
        _navigationService.SetContentFrame(ContentFrame);
        _navigationService.NavigateToContent<DashboardView>();
    }

    /// <summary>
    ///     Gets the most recently created <see cref="NavView" /> instance.
    ///     Used by content pages to call shell-level operations such as updating the notification badge.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public static NavView? Current { get; private set; }

    /// <summary>
    ///     Updates the notification badge on the bell icon to reflect the number of unread notifications.
    ///     Hides the badge entirely when <paramref name="count" /> is zero or negative.
    /// </summary>
    /// <param name="count">The number of unread notifications to display.</param>
    public void UpdateNotificationBadge(int count)
    {
        if (count <= default(int))
        {
            NotificationBadge.Visibility = Visibility.Collapsed;
            return;
        }

        NotificationBadgeText.Text = count > MaximumInlineNotificationBadgeCount
            ? OverflowNotificationBadgeText
            : count.ToString(CultureInfo.InvariantCulture);
        NotificationBadge.Visibility = Visibility.Visible;
    }

    /// <summary>
    ///     Shows a modal dialog informing the user that the given feature is not yet available.
    ///     Called by both sidebar buttons and content pages for unimplemented navigation targets.
    /// </summary>
    /// <param name="feature">The display name of the feature, shown as the dialog title and in the message body.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous dialog operation.</returns>
    public async Task ShowComingSoonAsync(string feature)
    {
        var dialog = new ContentDialog
        {
            Title = feature,
            Content = $"{feature} is coming soon.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    private void SetActiveNav(Button selected)
    {
        foreach (Button button in _navButtons)
        {
            button.Style = (Style)Resources["NavItemStyle"];
        }

        selected.Style = (Style)Resources["NavItemActiveStyle"];
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavDashboard);
        _navigationService.NavigateToContent<DashboardView>();
    }

    private void NavProfile_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavProfile);
        _navigationService.NavigateToContent<ProfileView>();
    }

    private void NavBeneficiaries_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavBeneficiaries);
        _navigationService.NavigateToContent<BeneficiariesView>();
    }

    // All other nav items show a coming soon alert
    private void NavTransfers_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavTransfers);
        _navigationService.NavigateToContent<TransferView>();
    }

    private void NavBillPayments_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavBillPayments);
        _navigationService.NavigateToContent<BillPayView>();
    }

    private void NavRecurringPayments_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavRecurringPayments);
        _navigationService.NavigateToContent<RecurringPaymentView>();
    }

    private async void NavCards_Click(object sender, RoutedEventArgs e)
    {
        await ShowComingSoonAsync("Cards");
    }

    private void NavTransferHistory_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavTransferHistory);
        _navigationService.NavigateToContent<TransferHistoryView>();
    }

    private void NavCurrencyExchange_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavCurrencyExchange);
        _navigationService.NavigateToContent<ForexPage>();
    }

    private void NavRateAlerts_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(NavRateAlerts);
        _navigationService.NavigateToContent<RateAlertsPage>();
    }

    private async void NavSavings_Click(object sender, RoutedEventArgs e)
    {
        await ShowComingSoonAsync("Savings & Loans");
    }

    private async void NavInvestments_Click(object sender, RoutedEventArgs e)
    {
        await ShowComingSoonAsync("Investments & Trading");
    }

    private async void NavStatistics_Click(object sender, RoutedEventArgs e)
    {
        await ShowComingSoonAsync("Statistics");
    }

    private async void NavSupport_Click(object sender, RoutedEventArgs e)
    {
        await ShowComingSoonAsync("Support");
    }

    private void NotificationBell_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        string message = NotificationBadge.Visibility == Visibility.Visible
            ? $"You have {NotificationBadgeText.Text} unread notifications."
            : "You have no unread notifications.";
        _ = ShowAlertAsync("Notifications", message);
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _apiClient.PostAsync<object>("/api/auth/logout", new { });
        }
        catch
        {
        }

        _apiClient.ClearToken();
        _navigationService.NavigateTo<LoginView>();
    }

    private async Task ShowAlertAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }
}
