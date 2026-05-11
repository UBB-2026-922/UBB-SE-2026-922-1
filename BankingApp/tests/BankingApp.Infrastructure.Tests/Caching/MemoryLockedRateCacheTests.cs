namespace BankingApp.Infrastructure.Tests.Caching;

public sealed class MemoryLockedRateCacheTests
{
    [Fact]
    public void TryGet_WhenRateHasNotBeenStored_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Store_WhenCalled_ShouldMakeRateRetrievableByUserId()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Store_WhenCalledTwiceForSameUser_ShouldOverwritePreviousRate()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Remove_WhenCalled_ShouldMakeRateUnretrievable()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void TryGet_WhenRateExpires_ShouldReturnNull()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void TryGet_WhenDifferentUserIds_ShouldReturnRatesIndependently()
    {
        throw new NotImplementedException();
    }
}
