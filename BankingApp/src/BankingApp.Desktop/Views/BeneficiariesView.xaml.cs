namespace BankingApp.Desktop.Views;

using Contracts.Features.Beneficiaries.Dtos;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        try
        {
            await ViewModel.LoadBeneficiariesAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
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
        try
        {
            await ViewModel.AddBeneficiaryAsync();
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDto? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null)
        {
            return;
        }

        try
        {
            await ViewModel.DeleteBeneficiaryAsync(beneficiary.Id);
        }
        catch
        {
            // ViewModel surfaces errors through its observable state.
        }
    }

    private void Use_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        BeneficiaryDto? beneficiary = TryGetBeneficiaryFromSender(sender);
        if (beneficiary is null)
        {
            return;
        }

        ViewModel.UseForTransfer(beneficiary);
    }

    private static BeneficiaryDto? TryGetBeneficiaryFromSender(object sender)
    {
        return sender is Button { Tag: BeneficiaryDto beneficiary } ? beneficiary : null;
    }
}
