namespace BankingApp.Web.Tests.ViewModels.Cards;

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
            Cards = [new CardRowViewModel()]
        };

        viewModel.HasCards.Should().BeTrue();
    }

    [Fact]
    public void HasCards_WhenCardsListHasMultipleEntries_ReturnsTrue()
    {
        CardListViewModel viewModel = new()
        {
            Cards = [new CardRowViewModel(), new CardRowViewModel(), new CardRowViewModel()]
        };

        viewModel.HasCards.Should().BeTrue();
    }
}
