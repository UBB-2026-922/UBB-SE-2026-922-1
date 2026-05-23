namespace BankingApp.Web.Tests.ViewModels.Cards;

using BankingApp.Domain.Enums;
using BankingApp.Web.ViewModels.Cards;

public sealed class CardRowViewModelTests
{
    [Theory]
    [InlineData(CardStatus.Active,    "bg-success")]
    [InlineData(CardStatus.Frozen,    "bg-warning text-dark")]
    [InlineData(CardStatus.Cancelled, "bg-secondary")]
    [InlineData(CardStatus.Expired,   "bg-danger")]
    public void StatusBadgeClass_ReturnsExpectedClass(CardStatus status, string expectedClass)
    {
        CardRowViewModel viewModel = new() { Status = status };

        viewModel.StatusBadgeClass.Should().Be(expectedClass);
    }

    [Fact]
    public void CanFreeze_WhenStatusIsActive_ReturnsTrue()
    {
        CardRowViewModel viewModel = new() { Status = CardStatus.Active };

        viewModel.CanFreeze.Should().BeTrue();
    }

    [Theory]
    [InlineData(CardStatus.Frozen)]
    [InlineData(CardStatus.Cancelled)]
    [InlineData(CardStatus.Expired)]
    public void CanFreeze_WhenStatusIsNotActive_ReturnsFalse(CardStatus status)
    {
        CardRowViewModel viewModel = new() { Status = status };

        viewModel.CanFreeze.Should().BeFalse();
    }

    [Fact]
    public void CanUnfreeze_WhenStatusIsFrozen_ReturnsTrue()
    {
        CardRowViewModel viewModel = new() { Status = CardStatus.Frozen };

        viewModel.CanUnfreeze.Should().BeTrue();
    }

    [Theory]
    [InlineData(CardStatus.Active)]
    [InlineData(CardStatus.Cancelled)]
    [InlineData(CardStatus.Expired)]
    public void CanUnfreeze_WhenStatusIsNotFrozen_ReturnsFalse(CardStatus status)
    {
        CardRowViewModel viewModel = new() { Status = status };

        viewModel.CanUnfreeze.Should().BeFalse();
    }

    [Theory]
    [InlineData(CardStatus.Active)]
    [InlineData(CardStatus.Frozen)]
    public void CanCancel_WhenStatusIsActiveOrFrozen_ReturnsTrue(CardStatus status)
    {
        CardRowViewModel viewModel = new() { Status = status };

        viewModel.CanCancel.Should().BeTrue();
    }

    [Theory]
    [InlineData(CardStatus.Cancelled)]
    [InlineData(CardStatus.Expired)]
    public void CanCancel_WhenStatusIsCancelledOrExpired_ReturnsFalse(CardStatus status)
    {
        CardRowViewModel viewModel = new() { Status = status };

        viewModel.CanCancel.Should().BeFalse();
    }
}
