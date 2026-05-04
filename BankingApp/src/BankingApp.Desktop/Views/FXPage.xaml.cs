// <copyright file="FXPage.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the FXPage code-behind.
// </summary>

using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Code-behind for the FX currency exchange page.
/// </summary>
public sealed partial class FXPage : Page
{
    private readonly FXViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FXPage"/> class.
    /// </summary>
    /// <param name="viewModel">The exchange view model injected by DI.</param>
    public FXPage(FXViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private async void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadPreviewAsync();
    }

    private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ExecuteExchangeAsync();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Reset();
    }
}
