namespace BankingApp.Infrastructure.Tests.ExchangeRates;

public sealed class ConfigurationExchangeRateServiceTests
{
    [Fact]
    public void GetRate_WhenBaseCurrencyEqualsQuoteCurrency_ShouldReturnOne()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetRate_WhenKnownCurrencyPairHasDefaultRate_ShouldReturnDefaultRate()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetRate_WhenConfigurationOverridesDefaultRate_ShouldReturnConfiguredRate()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetRate_WhenConfiguredRateIsInvalidDecimal_ShouldFallBackToDefaultRate()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetRate_WhenCurrencyPairIsUnknownAndNotConfigured_ShouldReturnNotFoundError()
    {
        throw new NotImplementedException();
    }
}
