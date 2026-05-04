// <copyright file="RateAlertsPage.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RateAlertsPage code-behind.
// </summary>

using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Code-behind for the Rate Alerts management page.
/// </summary>
public sealed partial class RateAlertsPage : Page
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
        await _viewModel.LoadAlertsAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAlertsAsync();
    }

    private async void CreateAlertButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.CreateAlertAsync();
    }

    private async void DeleteAlertButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int alertId)
        {
            await _viewModel.DeleteAlertAsync(alertId);
        }
    }
}
