namespace BankingApp.Application.Tests.Features.Forex.Queries;

public sealed class GetRatePreviewQueryTests
{
    [Fact]
    public void Handle_WhenCurrencyCodeIsInvalid_ShouldReturnInvalidCurrencyError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenSourceAndTargetCurrencyAreTheSame_ShouldReturnSameCurrencyError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldStoreLockedRateInCache()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldReturnPreviewWithCalculatedTargetAmount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldIncludeCommissionInPreview()
    {
        throw new NotImplementedException();
    }
}
