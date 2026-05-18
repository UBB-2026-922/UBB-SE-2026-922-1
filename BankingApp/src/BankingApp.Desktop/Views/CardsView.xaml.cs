namespace BankingApp.Desktop.Views;

using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Contracts.Features.Cards.Dtos;
using ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>Displays the card management screen.</summary>
public sealed partial class CardsView
{
    private readonly CardViewModel _viewModel;

    /// <summary>Initializes a new instance of the <see cref="CardsView"/> class.</summary>
    /// <param name="viewModel">The view model.</param>
    public CardsView(CardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += OnPageLoaded;
    }

    /// <summary>Gets the view model backing this page.</summary>
    public CardViewModel ViewModel => _viewModel;

    private async void OnPageLoaded(object sender, RoutedEventArgs args)
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        ErrorInfoBar.IsOpen = false;

        await _viewModel.LoadAsync();

        LoadingOverlay.Visibility = Visibility.Collapsed;

        if (_viewModel.HasError)
        {
            ErrorInfoBar.Message = _viewModel.ErrorMessage;
            ErrorInfoBar.IsOpen = true;
        }

        List<CardDetailsDto> cards = [.. _viewModel.Cards];
        CardsList.ItemsSource = cards;
        EmptyState.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void FreezeButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is not Button { Tag: CardDetailsDto card })
        {
            return;
        }

        await _viewModel.FreezeAsync(card);
        await RefreshAsync();
    }

    private async void UnfreezeButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is not Button { Tag: CardDetailsDto card })
        {
            return;
        }

        await _viewModel.UnfreezeAsync(card);
        await RefreshAsync();
    }

    private async void CancelButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is not Button { Tag: CardDetailsDto card })
        {
            return;
        }

        await _viewModel.CancelAsync(card);
        await RefreshAsync();
    }

    private void ShowIssueForm_Click(object sender, RoutedEventArgs args)
    {
        _viewModel.ShowIssueForm();
        IssueFormPanel.Visibility = Visibility.Visible;
    }

    private void HideIssueForm_Click(object sender, RoutedEventArgs args)
    {
        _viewModel.HideIssueForm();
        IssueFormPanel.Visibility = Visibility.Collapsed;
    }

    private async void SubmitIssueCard_Click(object sender, RoutedEventArgs args)
    {
        _viewModel.NewCardBrandIndex = CardBrandCombo.SelectedIndex;
        _viewModel.NewCardTypeIndex = CardTypeCombo.SelectedIndex;

        bool success = await _viewModel.IssueCardAsync();

        if (success)
        {
            IssueFormPanel.Visibility = Visibility.Collapsed;
            CardBrandCombo.SelectedIndex = 0;
            CardTypeCombo.SelectedIndex = 0;
            await RefreshAsync();
        }
        else if (_viewModel.HasError)
        {
            ErrorInfoBar.Message = _viewModel.ErrorMessage;
            ErrorInfoBar.IsOpen = true;
        }
    }

    private async Task RefreshAsync()
    {
        ErrorInfoBar.IsOpen = false;

        await _viewModel.LoadAsync();

        if (_viewModel.HasError)
        {
            ErrorInfoBar.Message = _viewModel.ErrorMessage;
            ErrorInfoBar.IsOpen = true;
        }

        List<CardDetailsDto> cards = [.. _viewModel.Cards];
        CardsList.ItemsSource = cards;
        EmptyState.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ToggleCardNumber_Click(object sender, RoutedEventArgs args)
    {
        if (sender is not Button { Tag: CardDetailsDto card } button)
        {
            return;
        }

        var row = button.Parent as StackPanel;
        var numberPanel = row?.Parent as StackPanel;
        const int cardNumberDisplayIndex = 0;
        var display = numberPanel?.Children[cardNumberDisplayIndex] as TextBlock;
        if (display is null)
        {
            return;
        }

        bool isRevealed = display.Text == card.FullCardNumber;
        display.Text = isRevealed ? card.CardNumber : card.FullCardNumber;
        button.Content = isRevealed ? "Show card number" : "Hide card number";
    }

    private void ToggleCvv_Click(object sender, RoutedEventArgs args)
    {
        if (sender is not Button { Tag: CardDetailsDto card } button)
        {
            return;
        }

        var row = button.Parent as StackPanel;
        TextBlock? cvvDisplay = null;
        if (row is not null)
        {
            foreach (UIElement child in row.Children)
            {
                if (child is TextBlock tb && tb != button)
                {
                    cvvDisplay = tb;
                    break;
                }
            }
        }

        if (cvvDisplay is null)
        {
            return;
        }

        bool isRevealed = cvvDisplay.Text == card.SecurityCode;
        cvvDisplay.Text = isRevealed ? string.Empty : card.SecurityCode;
        button.Content = isRevealed ? "Show security code" : "Hide security code";
    }
}
