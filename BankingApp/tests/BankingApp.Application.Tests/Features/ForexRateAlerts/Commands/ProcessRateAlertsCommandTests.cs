namespace BankingApp.Application.Tests.Features.ForexRateAlerts.Commands;

using BankingApp.Application.Features.Forex.Services;
using BankingApp.Application.Features.ForexRateAlerts.Commands;
using ErrorOr;
using Shared.Persistence;
using Currency = NodaMoney.Currency;

public sealed class ProcessRateAlertsCommandTests
{
    private const int TestUserId = 1;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRateAlertRepository> _rateAlertRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenNoUntriggeredAlerts_ShouldReturnSuccessWithoutUpdates()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        _rateAlertRepositoryMock
            .Setup(repository => repository.ListAllUntriggeredAsync(cancellationToken))
            .ReturnsAsync([]);

        ProcessRateAlertsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessRateAlertsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);

        _rateAlertRepositoryMock.Verify(repository => repository.ListAllUntriggeredAsync(cancellationToken), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAlertConditionIsMet_ShouldMarkAlertAsTriggered()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RateAlert alert = CreateRateAlert(isBuyAlert: true, targetRate: 1.25m);

        _rateAlertRepositoryMock
            .Setup(repository => repository.ListAllUntriggeredAsync(cancellationToken))
            .ReturnsAsync([alert]);

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(alert.BaseCurrency, alert.QuoteCurrency))
            .Returns((ErrorOr<decimal>)1.20m);

        _rateAlertRepositoryMock
            .Setup(repository => repository.UpdateAsync(alert, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        ProcessRateAlertsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessRateAlertsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);
        alert.IsTriggered.Should().BeTrue();

        _rateAlertRepositoryMock.Verify(repository => repository.ListAllUntriggeredAsync(cancellationToken), Times.Once);
        _exchangeRateServiceMock.Verify(service => service.GetRate(alert.BaseCurrency, alert.QuoteCurrency), Times.Once);
        _rateAlertRepositoryMock.Verify(repository => repository.UpdateAsync(alert, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAlertConditionIsMet_ShouldSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RateAlert alert = CreateRateAlert(isBuyAlert: false, targetRate: 1.25m);

        _rateAlertRepositoryMock
            .Setup(repository => repository.ListAllUntriggeredAsync(cancellationToken))
            .ReturnsAsync([alert]);

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(alert.BaseCurrency, alert.QuoteCurrency))
            .Returns((ErrorOr<decimal>)1.30m);

        _rateAlertRepositoryMock
            .Setup(repository => repository.UpdateAsync(alert, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        ProcessRateAlertsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessRateAlertsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        _rateAlertRepositoryMock.Verify(repository => repository.ListAllUntriggeredAsync(cancellationToken), Times.Once);
        _exchangeRateServiceMock.Verify(service => service.GetRate(alert.BaseCurrency, alert.QuoteCurrency), Times.Once);
        _rateAlertRepositoryMock.Verify(repository => repository.UpdateAsync(alert, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(cancellationToken), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenAlertConditionIsNotMet_ShouldNotMarkAlertAsTriggered()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        RateAlert alert = CreateRateAlert(isBuyAlert: true, targetRate: 1.25m);

        _rateAlertRepositoryMock
            .Setup(repository => repository.ListAllUntriggeredAsync(cancellationToken))
            .ReturnsAsync([alert]);

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(alert.BaseCurrency, alert.QuoteCurrency))
            .Returns((ErrorOr<decimal>)1.30m);

        ProcessRateAlertsCommandHandler handler = CreateHandler();

        // Act
        ErrorOr<int> result = await handler.Handle(new ProcessRateAlertsCommand(), cancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
        alert.IsTriggered.Should().BeFalse();

        _rateAlertRepositoryMock.Verify(repository => repository.ListAllUntriggeredAsync(cancellationToken), Times.Once);
        _exchangeRateServiceMock.Verify(service => service.GetRate(alert.BaseCurrency, alert.QuoteCurrency), Times.Once);
        _rateAlertRepositoryMock.VerifyNoOtherCalls();
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    private ProcessRateAlertsCommandHandler CreateHandler()
    {
        return new ProcessRateAlertsCommandHandler(
            _rateAlertRepositoryMock.Object,
            _exchangeRateServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    private static RateAlert CreateRateAlert(bool isBuyAlert, decimal targetRate)
    {
        return RateAlert.Create(
            TestUserId,
            Currency.FromCode("EUR"),
            Currency.FromCode("USD"),
            targetRate,
            isBuyAlert,
            _testNow).Value;
    }
}
