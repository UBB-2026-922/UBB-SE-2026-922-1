namespace BankingApp.Domain.Tests.Aggregates.RateAlertAggregate;

public sealed class RateAlertTests
{
    [Fact]
    public void Create_WhenBaseCurrencyEqualsQuoteCurrency_ShouldReturnMatchingCurrenciesError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenTargetRateIsZero_ShouldReturnInvalidTargetRateError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenTargetRateIsNegative_ShouldReturnInvalidTargetRateError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenValidParams_ShouldCreateSuccessfully()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ShouldTrigger_WhenBuyAlertAndCurrentRateIsAtOrBelowTarget_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ShouldTrigger_WhenBuyAlertAndCurrentRateIsAboveTarget_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ShouldTrigger_WhenSellAlertAndCurrentRateIsAtOrAboveTarget_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ShouldTrigger_WhenSellAlertAndCurrentRateIsBelowTarget_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ShouldTrigger_WhenRateDifferenceIsWithinRoundingPrecision_ShouldTreatRatesAsEqual()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void MarkTriggered_WhenCalled_ShouldSetIsTriggeredToTrue()
    {
        throw new NotImplementedException();
    }
}
