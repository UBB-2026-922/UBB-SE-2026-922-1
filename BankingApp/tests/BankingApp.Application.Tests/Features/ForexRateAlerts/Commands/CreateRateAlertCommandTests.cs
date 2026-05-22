namespace BankingApp.Application.Tests.Features.ForexRateAlerts.Commands;

using BankingApp.Application.Features.ForexRateAlerts.Commands;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.ForexRateAlerts.Dtos;
using ErrorOr;
using Shared.Clock;
using Shared.Persistence;

public sealed class CreateRateAlertCommandTests
{
    private const int TestUserId = 1;
    private const decimal TargetRate = 1.25m;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRateAlertRepository> _rateAlertRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenCurrenciesAreTheSame_ShouldReturnMatchingCurrenciesError()
    {
        // Arrange
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        CreateRateAlertCommandHandler handler = CreateHandler();
        var command = new CreateRateAlertCommand(TestUserId, "EUR", "EUR", TargetRate, true);

        // Act
        ErrorOr<ForexRateAlertDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RateAlertErrors.MatchingCurrencies);

        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenTargetRateIsInvalid_ShouldReturnInvalidTargetRateError()
    {
        // Arrange
        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        CreateRateAlertCommandHandler handler = CreateHandler();
        var command = new CreateRateAlertCommand(TestUserId, "EUR", "USD", 0m, true);

        // Act
        ErrorOr<ForexRateAlertDto> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(RateAlertErrors.InvalidTargetRate);

        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateAndPersistRateAlert()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RateAlert? persistedAlert = null;

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _rateAlertRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<RateAlert>(), cancellationToken))
            .Callback<RateAlert, CancellationToken>((alert, _) => persistedAlert = alert)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        CreateRateAlertCommandHandler handler = CreateHandler();
        var command = new CreateRateAlertCommand(TestUserId, "EUR", "USD", TargetRate, true);

        // Act
        ErrorOr<ForexRateAlertDto> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        persistedAlert.Should().NotBeNull();
        persistedAlert!.UserId.Should().Be(TestUserId);
        persistedAlert.BaseCurrency.Code.Should().Be("EUR");
        persistedAlert.QuoteCurrency.Code.Should().Be("USD");
        persistedAlert.TargetRate.Should().Be(TargetRate);
        persistedAlert.IsBuyAlert.Should().BeTrue();
        persistedAlert.IsTriggered.Should().BeFalse();
        persistedAlert.CreatedAt.Should().Be(_testNow);

        result.Value.UserId.Should().Be(TestUserId);
        result.Value.BaseCurrency.Should().Be("EUR");
        result.Value.TargetCurrency.Should().Be("USD");
        result.Value.TargetRate.Should().Be(TargetRate);
        result.Value.IsBuyAlert.Should().BeTrue();
        result.Value.IsTriggered.Should().BeFalse();
        result.Value.CreatedAt.Should().Be(_testNow);

        _rateAlertRepositoryMock.Verify(repository => repository.AddAsync(persistedAlert, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _rateAlertRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<RateAlert>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        CreateRateAlertCommandHandler handler = CreateHandler();
        var command = new CreateRateAlertCommand(TestUserId, "EUR", "USD", TargetRate, false);

        // Act
        ErrorOr<ForexRateAlertDto> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();

        _rateAlertRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RateAlert>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private CreateRateAlertCommandHandler CreateHandler()
    {
        return new CreateRateAlertCommandHandler(
            _rateAlertRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object);
    }
}
