namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.ForexRateAlerts.Commands;
using BankingApp.Application.Features.ForexRateAlerts.Dtos;
using BankingApp.Application.Features.ForexRateAlerts.Queries;
using ErrorOr;

public sealed class GetRateAlertsQueryHandlerTests
{
    private readonly Mock<IRateAlertRepository> _rateAlertRepo = MockFactory.CreateRateAlertRepository();

    private GetRateAlertsQueryHandler CreateHandler() => new(_rateAlertRepo.Object);

    [Fact]
    public async Task Handle_WhenNoAlerts_ReturnsEmptyList()
    {
        var query = new GetRateAlertsQuery(1);

        ErrorOr<List<ForexRateAlertDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}

public sealed class CreateRateAlertCommandHandlerTests
{
    private readonly Mock<IRateAlertRepository> _rateAlertRepo = MockFactory.CreateRateAlertRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private CreateRateAlertCommandHandler CreateHandler() => new(
        _rateAlertRepo.Object,
        _unitOfWork.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenInvalidCurrencyCode_ReturnsError()
    {
        var command = new CreateRateAlertCommand(1, "INVALID", "USD", 1.20m, true);

        ErrorOr<ForexRateAlertDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenInvalidTargetRate_ReturnsValidationError()
    {
        var command = new CreateRateAlertCommand(1, "EUR", "USD", 0m, true);

        ErrorOr<ForexRateAlertDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesAlertAndSaves()
    {
        var command = new CreateRateAlertCommand(1, "EUR", "USD", 1.20m, true);

        ErrorOr<ForexRateAlertDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.BaseCurrency.Should().Be("EUR");
        result.Value.TargetCurrency.Should().Be("USD");
        _rateAlertRepo.Verify(r => r.AddAsync(It.IsAny<RateAlert>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class DeleteRateAlertCommandHandlerTests
{
    private readonly Mock<IRateAlertRepository> _rateAlertRepo = MockFactory.CreateRateAlertRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private DeleteRateAlertCommandHandler CreateHandler() => new(
        _rateAlertRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenAlertNotFound_ReturnsNotFoundError()
    {
        var command = new DeleteRateAlertCommand(1, 99);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAlertBelongsToDifferentUser_ReturnsNotFoundError()
    {
        var alert = RateAlert.Create(2, NodaMoney.Currency.FromCode("EUR"), NodaMoney.Currency.FromCode("USD"), 1.20m, true, DateTime.UtcNow).Value;
        _rateAlertRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);
        var command = new DeleteRateAlertCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_DeletesAndSaves()
    {
        var alert = RateAlert.Create(1, NodaMoney.Currency.FromCode("EUR"), NodaMoney.Currency.FromCode("USD"), 1.20m, true, DateTime.UtcNow).Value;
        _rateAlertRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alert);
        var command = new DeleteRateAlertCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _rateAlertRepo.Verify(r => r.DeleteAsync(It.IsAny<RateAlert>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class ProcessRateAlertsCommandHandlerTests
{
    private readonly Mock<IRateAlertRepository> _rateAlertRepo = MockFactory.CreateRateAlertRepository();
    private readonly Mock<IExchangeRateService> _exchangeRateService = MockFactory.CreateExchangeRateService();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private ProcessRateAlertsCommandHandler CreateHandler() => new(
        _rateAlertRepo.Object,
        _exchangeRateService.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenNoUntriggeredAlerts_ReturnsZero()
    {
        var command = new ProcessRateAlertsCommand();

        ErrorOr<int> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenBuyAlertConditionMet_TriggersAndSaves()
    {
        // Buy alert triggers when currentRate <= targetRate; service returns 1.15m, so target 1.20m triggers
        var alert = RateAlert.Create(1, NodaMoney.Currency.FromCode("EUR"), NodaMoney.Currency.FromCode("USD"), 1.20m, true, DateTime.UtcNow).Value;
        _rateAlertRepo.Setup(r => r.ListAllUntriggeredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RateAlert>)new[] { alert });
        var command = new ProcessRateAlertsCommand();

        ErrorOr<int> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBuyAlertConditionNotMet_DoesNotTrigger()
    {
        // Buy alert: triggers when currentRate <= targetRate; service returns 1.15, target 1.10 means 1.15 <= 1.10 = false
        var alert = RateAlert.Create(1, NodaMoney.Currency.FromCode("EUR"), NodaMoney.Currency.FromCode("USD"), 1.10m, true, DateTime.UtcNow).Value;
        _rateAlertRepo.Setup(r => r.ListAllUntriggeredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<RateAlert>)new[] { alert });
        var command = new ProcessRateAlertsCommand();

        ErrorOr<int> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(0);
    }
}
