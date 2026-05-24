namespace BankingApp.Web.Tests.ViewModels.Cards;

using BankingApp.Contracts.Features.Cards.Dtos;
using BankingApp.Web.ViewModels.Cards;

public sealed class CardListViewModelTests
{
    [Fact]
    public void HasCards_WhenCardsListIsEmpty_ReturnsFalse()
    {
        CardListViewModel viewModel = new() { Cards = [] };

        viewModel.HasCards.Should().BeFalse();
    }

    [Fact]
    public void HasCards_WhenCardsListHasOneEntry_ReturnsTrue()
    {
        CardListViewModel viewModel = new()
        {
            Cards = [new CardDetailsDto()]
        };

        viewModel.HasCards.Should().BeTrue();
    }

    [Fact]
    public void HasCards_WhenCardsListHasMultipleEntries_ReturnsTrue()
    {
        CardListViewModel viewModel = new()
        {
            Cards = [new CardDetailsDto(), new CardDetailsDto(), new CardDetailsDto()]
        };

        viewModel.HasCards.Should().BeTrue();
    }
}

