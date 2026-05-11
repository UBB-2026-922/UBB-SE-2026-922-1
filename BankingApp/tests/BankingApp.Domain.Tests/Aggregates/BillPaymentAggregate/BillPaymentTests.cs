namespace BankingApp.Domain.Tests.Aggregates.BillPaymentAggregate;

public sealed class BillPaymentTests
{
    [Fact]
    public void Create_WhenAmountIsZero_ShouldReturnInvalidAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenAmountIsNegative_ShouldReturnInvalidAmountError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenFeeIsNegative_ShouldReturnInvalidFeeError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenCurrenciesMismatch_ShouldReturnCurrencyMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenBillerReferenceIsEmpty_ShouldReturnInvalidReferenceError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenBillerReferenceIsWhitespace_ShouldReturnInvalidReferenceError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenValidParams_ShouldCreateWithPendingStatus()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void RequiresTwoFactorAuthentication_WhenAmountMeetsThreshold_ShouldReturnTrue()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void RequiresTwoFactorAuthentication_WhenAmountIsBelowThreshold_ShouldReturnFalse()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void MarkProcessed_WhenCalled_ShouldSetStatusToCompleted()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void MarkProcessed_WhenCalled_ShouldSetReceiptNumber()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void MarkProcessed_WhenCalled_ShouldSetLedgerTransactionId()
    {
        throw new NotImplementedException();
    }
}
