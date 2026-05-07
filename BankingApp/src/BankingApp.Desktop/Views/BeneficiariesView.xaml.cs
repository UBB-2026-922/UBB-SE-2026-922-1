// <copyright file="BeneficiariesView.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiariesView page code-behind.
// </summary>

using System;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Beneficiaries;
using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Page that displays beneficiaries and basic CRUD interactions for the desktop app.
/// </summary>
public sealed partial class BeneficiariesView : Page
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiariesView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model instance to bind to the page.</param>
    public BeneficiariesView(BeneficiariesViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>
    ///     Gets the view model backing this page.
    /// </summary>
    public BeneficiariesViewModel ViewModel { get; }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadBeneficiariesAsync();
    }

    private void ShowAddForm_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsAddFormVisible = true;
    }

    private void CancelAdd_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsAddFormVisible = false;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.AddBeneficiaryAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is BeneficiaryDataTransferObject dto)
            await ViewModel.DeleteBeneficiaryAsync(dto.Id);
    }

    private void Use_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is BeneficiaryDataTransferObject dto)
            ViewModel.UseForTransfer(dto);
    }
}