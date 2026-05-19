namespace BankingApp.Desktop.Views;

using System;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
///     Code-behind for the Rate Alerts management page.
/// </summary>
public sealed partial class RateAlertsPage
{
    private readonly RateAlertViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RateAlertsPage"/> class.
    /// </summary>
    /// <param name="viewModel">The rate alert view model injected by DI.</param>
    public RateAlertsPage(RateAlertViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += RateAlertsPage_Loaded;
    }

    private async void RateAlertsPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.LoadAlertsAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.LoadAlertsAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private async void CreateAlertButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.CreateAlertAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private async void DeleteAlertButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int alertId })
        {
            return;
        }

        try
        {
            await _viewModel.DeleteAlertAsync(alertId);
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }
}
