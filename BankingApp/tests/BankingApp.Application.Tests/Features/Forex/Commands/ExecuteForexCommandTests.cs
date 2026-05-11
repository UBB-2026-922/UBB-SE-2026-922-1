namespace BankingApp.Application.Tests.Features.Forex.Commands;

public sealed class ExecuteForexCommandTests
{
    [Fact]
    public void Handle_WhenSourceAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenTargetAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenLockedRateNotFound_ShouldReturnRateExpiredError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenLockedRateCurrencyMismatches_ShouldReturnLockedRateMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenSourceAccountCurrencyMismatches_ShouldReturnAccountCurrencyMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenTargetAccountCurrencyMismatches_ShouldReturnAccountCurrencyMismatchError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenInsufficientFundsInSourceAccount_ShouldReturnInsufficientFundsError()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldDebitSourceAndCreditTargetAccount()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldMarkForexTransactionAsExecuted()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldRemoveLockedRateFromCache()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Handle_WhenValid_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
