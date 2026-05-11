namespace BankingApp.Domain.Tests.Aggregates.AccountAggregate.Entities;

public sealed class CardTests
{
    [Fact]
    public void GetMaskedNumber_WhenCardNumberIsLongEnough_ShouldShowLastFourDigits()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetMaskedNumber_WhenCardNumberIsTooShort_ShouldReturnFullyMasked()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void IsExpired_WhenExpiryDateIsInThePast_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void IsExpired_WhenExpiryDateIsInTheFuture_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Cancel_WhenCalled_ShouldSetStatusToCancelled()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Cancel_WhenCalled_ShouldSetCancelledAt()
    {
        throw new NotImplementedException();
    }
}
