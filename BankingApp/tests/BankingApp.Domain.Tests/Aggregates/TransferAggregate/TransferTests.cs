namespace BankingApp.Domain.Tests.Aggregates.TransferAggregate;

public sealed class TransferTests
{
    [Fact]
    public void Create_WhenRecipientNameIsEmpty_ShouldReturnInvalidRecipientNameError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenRecipientNameIsWhitespace_ShouldReturnInvalidRecipientNameError()
    {
        throw new NotImplementedException();
    }

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
    public void Create_WhenValidParams_ShouldCreateWithPendingStatus()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Create_WhenReferenceIsWhitespace_ShouldStoreNullReference()
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
    public void MarkExecuted_WhenCalled_ShouldSetStatusToCompleted()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void MarkExecuted_WhenCalled_ShouldSetLedgerTransactionId()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void MarkFailed_WhenCalled_ShouldSetStatusToFailed()
    {
        throw new NotImplementedException();
    }
}
