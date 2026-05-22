namespace BankingApp.Application.Tests.Features.Forex.Queries;

using BankingApp.Application.Features.Forex.Queries;
using BankingApp.Application.Features.Forex.Services;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Forex.Dtos;
using ErrorOr;
using Shared.Clock;
using Currency = NodaMoney.Currency;

public sealed class GetRatePreviewQueryTests
{
    private const int TestUserId = 1;
    private const decimal SourceAmount = 100m;
    private const decimal ExchangeRate = 1.25m;

    private static readonly DateTime _testNow = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock = new(MockBehavior.Strict);
    private readonly Mock<ILockedRateCache> _lockedRateCacheMock = new(MockBehavior.Strict);
    private readonly Mock<ISystemClock> _clockMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenCurrencyCodeIsInvalid_ShouldReturnInvalidCurrencyError()
    {
        // Arrange
        GetRatePreviewQueryHandler handler = CreateHandler();
        var query = new GetRatePreviewQuery(TestUserId, "INVALID", "USD", SourceAmount);

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.InvalidCurrency);

        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSourceAndTargetCurrencyAreTheSame_ShouldReturnSameCurrencyError()
    {
        // Arrange
        GetRatePreviewQueryHandler handler = CreateHandler();
        var query = new GetRatePreviewQuery(TestUserId, "USD", "USD", SourceAmount);

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.SameCurrency);

        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldStoreLockedRateInCache()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(sourceCurrency, targetCurrency))
            .Returns((ErrorOr<decimal>)ExchangeRate);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _lockedRateCacheMock
            .Setup(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        GetRatePreviewQueryHandler handler = CreateHandler();
        var query = new GetRatePreviewQuery(TestUserId, "EUR", "USD", SourceAmount);

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        _exchangeRateServiceMock.Verify(service => service.GetRate(sourceCurrency, targetCurrency), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnPreviewWithCalculatedTargetAmount()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(sourceCurrency, targetCurrency))
            .Returns((ErrorOr<decimal>)ExchangeRate);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _lockedRateCacheMock
            .Setup(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        GetRatePreviewQueryHandler handler = CreateHandler();
        var query = new GetRatePreviewQuery(TestUserId, "EUR", "USD", SourceAmount);

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SourceCurrency.Should().Be("EUR");
        result.Value.TargetCurrency.Should().Be("USD");
        result.Value.TargetAmount.Should().Be(125m);
        result.Value.ExchangeRate.Should().Be(ExchangeRate);

        _exchangeRateServiceMock.Verify(service => service.GetRate(sourceCurrency, targetCurrency), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldIncludeCommissionInPreview()
    {
        // Arrange
        var sourceCurrency = Currency.FromCode("EUR");
        var targetCurrency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(sourceCurrency, targetCurrency))
            .Returns((ErrorOr<decimal>)ExchangeRate);

        _clockMock.Setup(clock => clock.UtcNow).Returns(_testNow);

        _lockedRateCacheMock
            .Setup(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow));

        GetRatePreviewQueryHandler handler = CreateHandler();
        var query = new GetRatePreviewQuery(TestUserId, "EUR", "USD", SourceAmount);

        // Act
        ErrorOr<ForexRatePreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Commission.Should().Be(0.50m);

        _exchangeRateServiceMock.Verify(service => service.GetRate(sourceCurrency, targetCurrency), Times.Once);
        _lockedRateCacheMock.Verify(cache => cache.Store(TestUserId, sourceCurrency, targetCurrency, ExchangeRate, _testNow), Times.Once);
        _clockMock.Verify(clock => clock.UtcNow, Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
        _lockedRateCacheMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    private GetRatePreviewQueryHandler CreateHandler()
    {
        return new GetRatePreviewQueryHandler(
            _exchangeRateServiceMock.Object,
            _lockedRateCacheMock.Object,
            _clockMock.Object);
    }
}
