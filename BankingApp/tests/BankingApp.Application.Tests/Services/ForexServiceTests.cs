namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Forex.Commands;
using BankingApp.Application.Features.Forex.Dtos;
using BankingApp.Application.Features.Forex.Queries;
using ErrorOr;

public sealed class GetRatePreviewQueryHandlerTests
{
    private readonly Mock<IExchangeRateService> _exchangeRateService = MockFactory.CreateExchangeRateService();
    private readonly Mock<ILockedRateCache> _lockedRateCache = MockFactory.CreateLockedRateCache();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private GetRatePreviewQueryHandler CreateHandler() => new(
        _exchangeRateService.Object,
        _lockedRateCache.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenInvalidSourceCurrency_ReturnsError()
    {
        var query = new GetRatePreviewQuery(1, "INVALID", "USD", 100m);

        ErrorOr<ForexRatePreviewResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSameCurrencies_ReturnsError()
    {
        var query = new GetRatePreviewQuery(1, "EUR", "EUR", 100m);

        ErrorOr<ForexRatePreviewResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenExchangeRateFails_ReturnsError()
    {
        _exchangeRateService.Setup(s => s.GetRate(It.IsAny<NodaMoney.Currency>(), It.IsAny<NodaMoney.Currency>()))
            .Returns(Error.NotFound("forex.rate_not_found"));
        var query = new GetRatePreviewQuery(1, "EUR", "USD", 100m);

        ErrorOr<ForexRatePreviewResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_ReturnsPreviewAndStoresLockedRate()
    {
        var query = new GetRatePreviewQuery(1, "EUR", "USD", 100m);

        ErrorOr<ForexRatePreviewResponse> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.SourceCurrency.Should().Be("EUR");
        result.Value.TargetCurrency.Should().Be("USD");
        _lockedRateCache.Verify(c => c.Store(
            It.IsAny<int>(),
            It.IsAny<NodaMoney.Currency>(),
            It.IsAny<NodaMoney.Currency>(),
            It.IsAny<decimal>(),
            It.IsAny<DateTime>()), Times.Once);
    }
}

public sealed class ExecuteForexCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepo = MockFactory.CreateAccountRepository();
    private readonly Mock<IForexRepository> _forexRepo = MockFactory.CreateForexRepository();
    private readonly Mock<ILockedRateCache> _lockedRateCache = MockFactory.CreateLockedRateCache();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private ExecuteForexCommandHandler CreateHandler() => new(
        _accountRepo.Object,
        _forexRepo.Object,
        _lockedRateCache.Object,
        _unitOfWork.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenNoLockedRate_ReturnsRateExpiredError()
    {
        var command = new ExecuteForexCommand(1, 1, 2, "EUR", "USD", 100m);

        ErrorOr<ForexTransactionResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSameCurrencies_ReturnsError()
    {
        var command = new ExecuteForexCommand(1, 1, 2, "EUR", "EUR", 100m);

        ErrorOr<ForexTransactionResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenLockedRateCurrenciesMismatch_ReturnsError()
    {
        var lockedRate = new LockedRate(
            NodaMoney.Currency.FromCode("EUR"),
            NodaMoney.Currency.FromCode("GBP"),
            1.15m,
            DateTime.UtcNow);
        _lockedRateCache.Setup(c => c.TryGet(It.IsAny<int>())).Returns(lockedRate);
        var command = new ExecuteForexCommand(1, 1, 2, "EUR", "USD", 100m);

        ErrorOr<ForexTransactionResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ReturnsNotFoundError()
    {
        var lockedRate = new LockedRate(
            NodaMoney.Currency.FromCode("EUR"),
            NodaMoney.Currency.FromCode("USD"),
            1.15m,
            DateTime.UtcNow);
        _lockedRateCache.Setup(c => c.TryGet(It.IsAny<int>())).Returns(lockedRate);
        var command = new ExecuteForexCommand(1, 1, 2, "EUR", "USD", 100m);

        ErrorOr<ForexTransactionResponse> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}

public sealed class GetForexHistoryQueryHandlerTests
{
    private readonly Mock<IForexRepository> _forexRepo = MockFactory.CreateForexRepository();

    private GetForexHistoryQueryHandler CreateHandler() => new(_forexRepo.Object);

    [Fact]
    public async Task Handle_WhenNoHistory_ReturnsEmptyList()
    {
        var query = new GetForexHistoryQuery(1);

        ErrorOr<List<ForexTransactionResponse>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}
