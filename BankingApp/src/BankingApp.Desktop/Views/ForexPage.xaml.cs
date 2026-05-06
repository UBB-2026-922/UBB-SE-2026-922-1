using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Code-behind for the FX currency exchange page.
/// </summary>
public sealed partial class ForexPage
{
    private readonly ForexViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ForexPage"/> class.
    /// </summary>
    /// <param name="viewModel">The exchange view model injected by DI.</param>
    public ForexPage(ForexViewModel viewModel)
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
