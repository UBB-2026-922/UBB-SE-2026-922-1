namespace BankingApp.Application.Tests.Features.Transfers.Queries;

using BankingApp.Application;
using BankingApp.Application.Features.Transfers.Queries;
using BankingApp.Domain.Common.Errors;
using Contracts.Features.Transfers.Dtos;
using ErrorOr;
using Currency = NodaMoney.Currency;

public sealed class GetForexPreviewQueryTests
{
    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock = new(MockBehavior.Strict);

    [Fact]
    public async Task Handle_WhenCurrencyCodeIsInvalid_ShouldReturnInvalidCurrencyError()
    {
        // Arrange
        GetForexPreviewQueryHandler handler = CreateHandler();
        var query = new GetForexPreviewQuery("INVALID", "USD", 100m);

        // Act
        ErrorOr<TransferForexPreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.InvalidCurrency);

        _exchangeRateServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSourceAndTargetCurrencyAreTheSame_ShouldReturnSameCurrencyError()
    {
        // Arrange
        var currency = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(currency, currency))
            .Returns((ErrorOr<decimal>)ForexErrors.SameCurrency);

        GetForexPreviewQueryHandler handler = CreateHandler();
        var query = new GetForexPreviewQuery("USD", "USD", 100m);

        // Act
        ErrorOr<TransferForexPreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ForexErrors.SameCurrency);

        _exchangeRateServiceMock.Verify(service => service.GetRate(currency, currency), Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnPreviewWithCalculatedConvertedAmount()
    {
        // Arrange
        var source = Currency.FromCode("EUR");
        var target = Currency.FromCode("USD");

        _exchangeRateServiceMock
            .Setup(service => service.GetRate(source, target))
            .Returns((ErrorOr<decimal>)1.25m);

        GetForexPreviewQueryHandler handler = CreateHandler();
        var query = new GetForexPreviewQuery("EUR", "USD", 100m);

        // Act
        ErrorOr<TransferForexPreviewResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ExchangeRate.Should().Be(1.25m);
        result.Value.ConvertedAmount.Should().Be(125m);

        _exchangeRateServiceMock.Verify(service => service.GetRate(source, target), Times.Once);
        _exchangeRateServiceMock.VerifyNoOtherCalls();
    }

    private GetForexPreviewQueryHandler CreateHandler()
    {
        return new GetForexPreviewQueryHandler(_exchangeRateServiceMock.Object);
    }
}
