namespace BankingApp.Application.Tests.Features.BillPayments.Commands;

public sealed class ProcessBillPaymentCommandTests
{
    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenAccountNotFound_ShouldReturnAccountNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenAccountBelongsToDifferentUser_ShouldReturnAccountNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenAccountIsNotActive_ShouldReturnAccountNotActiveError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenBillerNotFound_ShouldReturnBillerNotFoundError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenAmountAboveThresholdAndTwoFaTokenMissing_ShouldReturnTwoFaRequiredError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenAmountAboveThresholdAndTwoFaTokenIsInvalid_ShouldReturnInvalidTwoFaTokenError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenValid_ShouldDebitAccountAndCreateBillPaymentTransaction()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenValid_ShouldMarkBillPaymentAsProcessed()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented yet.")]
    public void Handle_WhenValid_ShouldSaveChanges()
    {
        throw new NotImplementedException();
    }
}
