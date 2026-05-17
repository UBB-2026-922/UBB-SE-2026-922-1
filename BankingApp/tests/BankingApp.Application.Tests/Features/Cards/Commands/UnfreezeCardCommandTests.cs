namespace BankingApp.Application.Tests.Features.Cards.Commands;

using BankingApp.Application.Common.Utilities;
using BankingApp.Application.Features.Cards.Commands;
using BankingApp.Domain.Common.Errors;
using Microsoft.Extensions.Logging;
using NodaMoney;

public sealed class UnfreezeCardCommandTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UnfreezeCardCommandHandler>> _loggerMock;
    private readonly UnfreezeCardCommandHandler _handler;

    public UnfreezeCardCommandTests()
    {
        _accountRepositoryMock = MockFactory.CreateAccountRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _loggerMock = new Mock<ILogger<UnfreezeCardCommandHandler>>();
        _handler = new UnfreezeCardCommandHandler(_accountRepositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ShouldReturnNotFoundError()
    {
        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Account>());

        var command = new UnfreezeCardCommand(UserId: 1, CardId: 99);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCardIsNotFrozen_ShouldReturnNotFrozenError()
    {
        Account account = Account.Open(1, null!, Currency.FromCode("USD"), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard("1234567890123456", "John Doe", DateTime.UtcNow.AddYears(2), "123", CardType.Debit, "Visa", DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { account });

        var command = new UnfreezeCardCommand(UserId: 1, CardId: card.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.NotFrozen);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUnfreezeCardAndSaveChanges()
    {
        Account account = Account.Open(1, null!, Currency.FromCode("USD"), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard("1234567890123456", "John Doe", DateTime.UtcNow.AddYears(2), "123", CardType.Debit, "Visa", DateTime.UtcNow);
        card.Freeze();

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { account });

        var command = new UnfreezeCardCommand(UserId: 1, CardId: card.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        card.Status.Should().Be(CardStatus.Active);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
