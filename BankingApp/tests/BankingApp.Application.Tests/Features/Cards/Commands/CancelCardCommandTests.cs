namespace BankingApp.Application.Tests.Features.Cards.Commands;

using BankingApp.Application.Common.Utilities;
using BankingApp.Application.Features.Cards.Commands;
using BankingApp.Domain.Common.Errors;
using Microsoft.Extensions.Logging;
using NodaMoney;

public sealed class CancelCardCommandTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISystemClock> _clockMock;
    private readonly Mock<ILogger<CancelCardCommandHandler>> _loggerMock;
    private readonly CancelCardCommandHandler _handler;

    public CancelCardCommandTests()
    {
        _accountRepositoryMock = MockFactory.CreateAccountRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _clockMock = MockFactory.CreateSystemClockMock();
        _loggerMock = new Mock<ILogger<CancelCardCommandHandler>>();
        _handler = new CancelCardCommandHandler(_accountRepositoryMock.Object, _unitOfWorkMock.Object, _clockMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ShouldReturnNotFoundError()
    {
        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Account>());

        var command = new CancelCardCommand(UserId: 1, CardId: 99);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCardAlreadyCancelled_ShouldReturnAlreadyCancelledError()
    {
        Account account = Account.Open(1, null!, Currency.FromCode("USD"), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard("1234567890123456", "John Doe", DateTime.UtcNow.AddYears(2), "123", CardType.Debit, "Visa", DateTime.UtcNow);
        card.Cancel(DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { account });

        var command = new CancelCardCommand(UserId: 1, CardId: card.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.AlreadyCancelled);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCancelCardAndSaveChanges()
    {
        Account account = Account.Open(1, null!, Currency.FromCode("USD"), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard("1234567890123456", "John Doe", DateTime.UtcNow.AddYears(2), "123", CardType.Debit, "Visa", DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { account });

        var command = new CancelCardCommand(UserId: 1, CardId: card.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        card.Status.Should().Be(CardStatus.Cancelled);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
