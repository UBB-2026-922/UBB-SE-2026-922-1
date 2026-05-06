// <copyright file="BeneficiariesView.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiariesView page code-behind.
// </summary>

using BankingApp.Application.DTOs.Beneficiaries;
using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Page that displays beneficiaries and basic CRUD interactions for the desktop app.
/// </summary>
public sealed partial class BeneficiariesView
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

    private async void OnPageLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        await ViewModel.LoadBeneficiariesAsync();
    }

    private void ShowAddForm_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        ViewModel.IsAddFormVisible = true;
    }

    private void CancelAdd_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        ViewModel.IsAddFormVisible = false;
    }

    private async void Save_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        await ViewModel.AddBeneficiaryAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDataTransferObject? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null) return;

        await ViewModel.DeleteBeneficiaryAsync(beneficiary.Id);
    }

    private void Use_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDataTransferObject? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null) return;

        ViewModel.UseForTransfer(beneficiary);
    }

    private static BeneficiaryDataTransferObject? TryGetBeneficiaryFromSender(object sender)
    {
        return sender is Button { Tag: BeneficiaryDataTransferObject beneficiary } ? beneficiary : null;
    }
}
