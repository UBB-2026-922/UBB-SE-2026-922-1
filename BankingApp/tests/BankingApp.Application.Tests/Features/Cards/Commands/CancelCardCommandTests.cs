namespace BankingApp.Application.Tests.Features.Cards.Commands;

using Common.Utilities;
using BankingApp.Application.Features.Cards.Commands;
using BankingApp.Domain.Common.Errors;
using ErrorOr;
using Microsoft.Extensions.Logging;
using NodaMoney;

public sealed class CancelCardCommandTests
{
    private const string TestCardNumber = "1234567890123456";
    private const string TestCvv = "123";
    private const string TestCardholderName = "John Doe";
    private const string TestCurrency = "USD";
    private const string TestCardBrand = "Visa";
    private const int CardExpiryYears = 2;

    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelCardCommandHandler _handler;

    public CancelCardCommandTests()
    {
        _accountRepositoryMock = MockFactory.CreateAccountRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        Mock<ISystemClock> clockMock = MockFactory.CreateSystemClockMock();
        var loggerMock = new Mock<ILogger<CancelCardCommandHandler>>();
        _handler = new CancelCardCommandHandler(_accountRepositoryMock.Object, _unitOfWorkMock.Object, clockMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCardNotFound_ShouldReturnNotFoundError()
    {
        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Account>());

        var command = new CancelCardCommand(UserId: 1, CardId: 99);

        ErrorOr<Success> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCardAlreadyCancelled_ShouldReturnAlreadyCancelledError()
    {
        var account = Account.Open(1, null!, Currency.FromCode(TestCurrency), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard(TestCardNumber, TestCardholderName, DateTime.UtcNow.AddYears(CardExpiryYears), TestCvv, CardType.Debit, TestCardBrand, DateTime.UtcNow);
        card.Cancel(DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var command = new CancelCardCommand(UserId: 1, CardId: card.Id);

        ErrorOr<Success> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CardErrors.AlreadyCancelled);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCancelCardAndSaveChanges()
    {
        var account = Account.Open(1, null!, Currency.FromCode(TestCurrency), AccountType.Checking, null, DateTime.UtcNow);
        Card card = account.IssueCard(TestCardNumber, TestCardholderName, DateTime.UtcNow.AddYears(CardExpiryYears), TestCvv, CardType.Debit, TestCardBrand, DateTime.UtcNow);

        _accountRepositoryMock
            .Setup(r => r.ListByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var command = new CancelCardCommand(UserId: 1, CardId: card.Id);

        ErrorOr<Success> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        card.Status.Should().Be(CardStatus.Cancelled);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
