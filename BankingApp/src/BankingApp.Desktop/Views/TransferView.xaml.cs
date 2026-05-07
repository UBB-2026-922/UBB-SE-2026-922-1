// <copyright file="TransferView.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains code for TransferView.xaml.
// </summary>

using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Hosts the multi-step transfer wizard.
///     The view receives its <see cref="TransferViewModel" /> through constructor injection
///     and triggers account loading when the page is displayed.
/// </summary>
public sealed partial class TransferView : Page
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives the transfer wizard.</param>
    public TransferView(TransferViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>
    ///     Gets the view model bound to this view.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public TransferViewModel ViewModel { get; }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _ = ViewModel.LoadAccountsAsync();
    }
}